using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace DriverCapsuleStressTool
{
    class ExecuteFromList
    {
        internal static List<string> capList = new List<string>();
        internal static bool rebootRequired = false;

        /// <summary>
        /// executes the stress in the order given by the list that was created
        /// only changed during execution when it finds firmware or the users choice to start first
        /// also groups firmware if chosen at runtime to install all firmware and reboot only once rather
        /// than rebooting for each firmware install seperately
        /// </summary>
        /// <param name="randomize"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="InputTestFilePath"></param>
        /// <param name="supportFolderLOC"></param>
        /// <param name="seedFilePath"></param>
        /// <param name="startChoice"></param>
        internal static void ExecuteTheList(bool randomize, int executionCount, string startChoice)
        {
            Process devMgr = new Process();
            devMgr.StartInfo.FileName = @"C:\Windows\System32\mmc.exe";
            devMgr.StartInfo.Arguments = "devmgmt.msc";
            devMgr.Start();
            
            try
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Waiting 3 seconds to be sure device is up and running...");
                Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(3000);
                if (RegCheck.IsRebootPending())
                {
                    Logger.Comment("there is a pending reboot...");
                    RebootAndContinue.RebootCmd(true);
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Checking for last installed...");
                Console.WriteLine("if it exists we'll check that it was successfull then...");
                Console.WriteLine("... ExecuteTheList...");
                Console.ForegroundColor = ConsoleColor.White;

                // check if INF previously installed was actually fully successful
                if (File.Exists(Program.lastInstalled))
                {
                    string tmpLine = File.ReadAllText(Program.lastInstalled);
                    GetData.IsInstalledAfterReboot(tmpLine);
                }

                int infListCount = XMLReader.GetInfsPathListCount(Program.InputTestFilePathBAK);
                string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                if (string.IsNullOrEmpty(infIndexListString))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("infListCount = " + infListCount);
                    Console.ForegroundColor = ConsoleColor.White;
                    executionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath);
                    DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                }

                int TMPexecutionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath); 
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("executionCount : " + TMPexecutionCount);
                Console.WriteLine("time to check the executionCount and decide what to do...");
                Console.ForegroundColor = ConsoleColor.White;

                if (TMPexecutionCount == 0)
                {
                    Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("at this point...I think I am done...am I?");
                    Console.WriteLine("-----------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;
                    ParseForResults.ParseFromdpinstLog(@"C:\Windows\DPINST.LOG");
                    File.Create(Program.dirName + @"\DONE.TXT");
                    CheckWhatInstalled.CheckInstalledCSV();
                    Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
                    Utilities.CopyFile(Program.InputTestFilePathBAK, Program.resultsLogDir + @"\DriverCapsuleStress.xml.BAK");
                    Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
                    Utilities.CopyFile(Program.dirName + @"\LastInstalled.txt", Program.resultsLogDir + @"\LastInstalled.txt");
                    Thread.Sleep(5000);
                    Console.WriteLine("Everything should be completed including copying logs to the Reults folder on the desktop");
                    Console.ReadKey();
                    Logger.FunctionLeave();
                }

                else
                {
                    List<string> DriverPathList = new List<string>();
                    string seedStr = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    XDocument xdoc = XDocument.Load(Program.InputTestFilePath);
                    infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    Console.WriteLine("infIndexListString = " + infIndexListString);

                    if (randomize)
                    {
                        executionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);
                            if (index.Equals(null)) { continue; }
                            DriverPathList = GetData.GetInfPathsList(Program.dirName);
                            string testIsStartChoice = GetData.GetTestFirst(Program.InputTestFilePath);
                            string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                            string line = XMLReader.FromINFIndex(infListCount, Program.InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileName(line);

                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                            if (isCapsule)
                            {
                                Logger.Comment("this is firmware treat it as such and reboot or rollback\reboot...");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                            }
                            else if (infName.Contains("Surface"))
                            {
                                Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                            }
                            else
                            {
                                Logger.Comment("this is NOT firmware check for reboot afer installed");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                break;
                            }
                        }
                        //Thread.Sleep(500);
                    }

                    else
                    {
                        // add ability to group install all firmware together with only one reboot
                        // in this case the firmware would all install individually but have only one reboot
                        // user should see each different color bar during reboot\install of all firmware
                        bool groupFirmware = XMLReader.GetGroupFirmware(Program.InputTestFilePathBAK);
                        int capListCount = 0;

                        if (groupFirmware)
                        {
                            executionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath);
                            foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                            {
                                Console.WriteLine("if (groupFirmware) - seedIndex : " + seedIndex);
                                string line = XMLReader.FromINFIndex(infListCount, Program.InputTestFilePath, seedIndex, executionCount).ToLower();
                                Console.WriteLine("if (groupFirmware) - line : " + line);
                                bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                                if (isCapsule)
                                {
                                    capListCount++;
                                    capList.Add(line);
                                }
                                else { continue; }
                            }

                            while (capListCount > 1)
                            {
                                foreach (string groupedFirmware in capList)
                                {
                                    {
                                        string seedIndexSTR = XMLReader.IndexFromINF(Program.InputTestFilePath, groupedFirmware);
                                        int seedIndex = Convert.ToInt32(seedIndexSTR);
                                        XMLWriter.RemoveXMLElemnt(Program.InputTestFilePath, groupedFirmware, seedIndex);
                                        Console.WriteLine("seedIndex : " + seedIndex);

                                        string friendlyDriverName = XMLReader.GetFriendlyDriverName(Program.InputTestFilePath, groupedFirmware);
                                        string hardwareID = GetData.FirmwareInstallGetHID(groupedFirmware);
                                        string expectedDriverVersion = GetData.GetDriverVersion(groupedFirmware);
                                        Logger.Comment("IfIsCapsule From RegCheck before IF : " + expectedDriverVersion);
                                        string infNameToTest = Path.GetFileNameWithoutExtension(groupedFirmware);
                                        string expectedDriverDate = GetData.GetDriverDate(groupedFirmware);
                                        bool isInstalled = CheckWhatInstalled.CheckInstalled(groupedFirmware, hardwareID, friendlyDriverName, infNameToTest, expectedDriverVersion, expectedDriverDate);

                                        if (isInstalled)
                                        {
                                            string infFileContent = File.ReadAllText(groupedFirmware).ToUpper();
                                            string infName = Path.GetFileNameWithoutExtension(groupedFirmware);
                                            SafeNativeMethods.RollbackInstall(seedIndex, groupedFirmware, infName, infFileContent, hardwareID, rebootRequired = true, Program.InputTestFilePath);
                                        }
                                        else
                                        {
                                            string groupedFirmwareDIR = Path.GetDirectoryName(groupedFirmware);
                                            string installArgs = " /C /A /Q /SE /F /PATH " + groupedFirmwareDIR;
                                            SafeNativeMethods.Install_Inf(groupedFirmware, Program.installer, installArgs, seedIndex);
                                        }
                                    }
                                }
                                RebootAndContinue.RebootCmd(true);
                            }
                        }

                        executionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);
                            if (index.Equals(null)) { continue; }
                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");

                            string line = XMLReader.FromINFIndex(infListCount, Program.InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileNameWithoutExtension(line);
                            string testIsStartChoice = GetData.GetTestFirst(Program.InputTestFilePath);
                            if (testIsStartChoice.Equals("none")) { break; }
                            string testInfName = infName.ToLower();
                            infIndexListString = File.ReadAllText(Program.seedFilePath);

                            if (line.Contains(testIsStartChoice.ToLower()))
                            {
                                Logger.Comment("This is the start first choice : " + line);
                                bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                                if (isCapsule)
                                {
                                    infListCount--;
                                    Logger.Comment("re-add the reg key to start post reboot...");
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                }
                                else if (infName.Contains("Surface"))
                                {
                                    Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("NOT MATCHING to the startChoice");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                    break;
                                }
                            }
                        }
                        //Thread.Sleep(1000);

                        infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);

                            //Thread.Sleep(100);
                            string indexString = Convert.ToString(index);
                            if (index.Equals(null)) { continue; }

                            string line = XMLReader.FromINFIndex(infListCount, Program.InputTestFilePath, seedIndex, executionCount).ToLower();
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            string infName = Path.GetFileName(line);
                            string testIsStartChoice = GetData.GetTestFirst(Program.InputTestFilePath);
                            string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                            //Thread.Sleep(500);

                            if (isCapsule)
                            {
                                Logger.Comment("this is firmware and will need to reboot...");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                            }
                            else if (infName.Contains("Surface"))
                            {
                                Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                            }
                            else
                            {
                                Logger.Comment("THIS IS NOT FIRMWARE...");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                break;
                            }
                        }
                        //Thread.Sleep(500);
                    }
                    executionCount--;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("executionCount :: " + executionCount);
                    Console.WriteLine("outside of the while loop");
                    Console.WriteLine("should be getting smaller correctly");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}