using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
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
        /// only changed during execution when it finds firmware etc.
        /// </summary>
        /// <param name="randomize"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="InputTestFilePath"></param>
        /// <param name="supportFolderLOC"></param>
        /// <param name="seedFilePath"></param>
        /// <param name="startChoice"></param>
        internal static void ExecuteTheList(bool randomize, int executionCount, string dirName, string InputTestFilePath, string supportFolderLOC, string seedFilePath, string startChoice)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Waiting 3 seconds to be sure device is up and running...");
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(3000);
            if (RegCheck.IsRebootPending())
            {
                Logger.Comment("there is a pending reboot...");
                Thread.Sleep(1000);
                RebootAndContinue.RebootCmd(true);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("... ExecuteTheList...");
            Console.ForegroundColor = ConsoleColor.White;

            if (executionCount == 0)
            {
                Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.resultsLogDir);
                Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("at this point...I think I am done...am I?");
                Console.WriteLine("-----------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                ParseForResults.ParseFromdpinstLog(Program.resultsLogDir);
                File.Create(Program.dirName + @"\DONE.TXT");
                CheckWhatInstalled.CheckInstalledCSV();
                Console.ReadKey();
                Logger.FunctionLeave();
            }

            try
            {
                while (executionCount > 0)
                {
                    List<string> DriverPathList = new List<string>();
                    string seedStr = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    if (!File.Exists(Program.dirName + @"\executionCount_" + executionCount + ".txt"))
                    {
                        File.WriteAllText(Program.dirName + @"\executionCount_" + executionCount + ".txt", seedStr);
                    }
                    if (File.Exists(Program.dirName + @"\" + executionCount + "*.txt"))
                    {
                        File.Delete(Program.dirName + @"\" + executionCount + "*.txt");
                        File.WriteAllText(Program.dirName + @"\executionCount_" + executionCount + ".txt", seedStr);
                    }

                    string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    Thread.Sleep(500);
                    XDocument xdoc = XDocument.Load(Program.InputTestFilePath);
                    int infListCount = XMLReader.GetInfsPathListCount(Program.InputTestFilePathBAK);

                    if (string.IsNullOrEmpty(infIndexListString))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("infListCount = " + infListCount);
                        Console.ForegroundColor = ConsoleColor.White;

                        if (randomize)
                        {
                            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                            DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                        }
                        else
                        {
                            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                            DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                        }
                    }

                    infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    Console.WriteLine("infIndexListString = " + infIndexListString);

                    if (randomize)
                    {
                        executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);
                            Thread.Sleep(100);
                            if (index.Equals(null)) { continue; }
                            DriverPathList = GetData.GetInfPathsList(dirName);
                            string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                            string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                            Thread.Sleep(500);
                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileName(line);

                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                            if (isCapsule)
                            {
                                Logger.Comment("this is firmware treat it as such and reboot or rollback\reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else if (infName.Contains("Surface"))
                            {
                                Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else
                            {
                                Logger.Comment("this is NOT firmware check for reboot afer installed");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                break;
                            }
                        }
                        Thread.Sleep(500);
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
                            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                            foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                            {
                                Console.WriteLine("if (groupFirmware) - seedIndex : " + seedIndex);

                                string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount).ToLower();

                                Console.WriteLine("if (groupFirmware) - line : " + line);
                                //Console.ReadKey();

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
                                        XMLWriter.RemoveXMLElemnt(InputTestFilePath, groupedFirmware, seedIndex);
                                        Console.WriteLine("seedIndex : " + seedIndex);

                                        string friendlyDriverName = XMLReader.GetFriendlyDriverName(InputTestFilePath, groupedFirmware);
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
                                            SafeNativeMethods.RollbackInstall(seedIndex, groupedFirmware, infName, infFileContent, hardwareID, rebootRequired = true, InputTestFilePath);
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

                        executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);
                            Thread.Sleep(100);
                            if (index.Equals(null)) { continue; }
                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");

                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileNameWithoutExtension(line);
                            string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                            string testInfName = infName.ToLower();
                            infIndexListString = File.ReadAllText(seedFilePath);

                            if (line.Contains(testIsStartChoice.ToLower()))
                            {
                                Logger.Comment("This is the start first choice : " + line);
                                bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                                if (isCapsule)
                                {
                                    infListCount--;
                                    Logger.Comment("re-add the reg key to start post reboot...");
                                    Thread.Sleep(500);
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                }
                                else if (infName.Contains("Surface"))
                                {
                                    Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                    Thread.Sleep(500);
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("NOT MATCHING to the startChoice");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(1000);

                        infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);

                            Thread.Sleep(100);
                            string indexString = Convert.ToString(index);
                            if (index.Equals(null)) { continue; }

                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount).ToLower();
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            string infName = Path.GetFileName(line);
                            string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                            string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                            Thread.Sleep(500);

                            if (isCapsule)
                            {
                                Logger.Comment("this is firmware and will need to reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else if (infName.Contains("Surface"))
                            {
                                Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else
                            {
                                Logger.Comment("THIS IS NOT FIRMWARE...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                break;
                            }
                        }
                        Thread.Sleep(500);
                    }
                    executionCount--;
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}