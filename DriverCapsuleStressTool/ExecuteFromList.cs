using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
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
                List<string> DriverPathList = GetData.GetInfPathsList(Program.dirName);
                CheckWhatInstalled.CheckInstalledCSV();

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

                // check if lastInstalled in the XML was successfully installed post-reboot
                string tmpLine = XMLReader.GetLastInstalled();
                string installCheck = GetData.IsInstalledAfterReboot(tmpLine);

                switch (installCheck)
                {
                    case string unsuccessful when installCheck.Equals("unsuccessful"):
                        DoThisIfFailedPostReboot();
                        break;

                    case string InsufficientResources when installCheck.Equals("InsufficientResources"):
                        DoThisIfFailedPostReboot();
                        break;

                    case string IncorrectVersion when installCheck.Equals("IncorrectVersion"):
                        DoThisIfFailedPostReboot();
                        break;

                    case string invalidImage when installCheck.Equals("invalidImage"):
                        DoThisIfFailedPostReboot();
                        break;

                    case string authenticationERR when installCheck.Equals("authenticationERR"):
                        DoThisIfFailedPostReboot();
                        break;

                    case string ACnotConnected when installCheck.Equals("ACnotConnected"):
                        DoThisIfFailedPostReboot();
                        break;

                    case string insufficientPower when installCheck.Equals("insufficientPower"):
                        DoThisIfFailedPostReboot();
                        break;

                    default:
                        installCheck.Equals("pass");
                        break;
                }
                
                // making sure there is a driver path to test in the XML file
                // if they have all been removed they are all done and we need to rewrite the XML
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

                // double check the current executin count in the XML to be sure we proceed correctly
                int TMPexecutionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath); 
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("executionCount : " + TMPexecutionCount);
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("time to check the executionCount and decide what to do...");
                Console.WriteLine("...waiting 10 seconds for Windows to catch up to me......");
                Console.WriteLine("----------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(10000);

                // if the execution count is now zero it is time to collect logs etc
                if (TMPexecutionCount == 0)
                {
                    Console.WriteLine("waiting before continue to see if this is where it is dying....sometimes...");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("at this point...I think I am done...am I?");
                    Console.WriteLine("-----------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;
                    ParseForResults.ParseFromdpinstLog(@"C:\Windows\DPINST.LOG");
                    File.Create(Program.dirName + @"\DONE.TXT");
                    CheckWhatInstalled.CheckInstalledCSV();
                    Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                    Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
                    Utilities.CopyFile(Program.InputTestFilePathBAK, Program.resultsLogDir + @"\DriverCapsuleStress.xml.BAK");
                    Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
                    Thread.Sleep(5000);
                    Console.WriteLine("Everything should be completed including copying logs to the Reults folder on the desktop");
                    // is the WTT service stopped.
                    StartStopServices.StartService("wttsvc");
                    Logger.FunctionLeave();
                }

                while (TMPexecutionCount >= 1)
                {
                    string seedStr = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    XDocument xdoc = XDocument.Load(Program.InputTestFilePath);
                    infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    Console.WriteLine("infIndexListString = " + infIndexListString);

                    if (randomize)
                    {
                        // if random is set to true this will take the list of index' for each INF
                        // into a list and shuffle the numbers out of order randomly using random seed
                        // then run through the list in that new order
                        // this will be done uniquely each time through the list for the executionCount
                        executionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            if (RegCheck.IsRebootPending())
                            {
                                Logger.Comment("there is a pending reboot...");
                                Thread.Sleep(3000);
                                RebootAndContinue.RebootCmd(true);
                            }
                            string index = Convert.ToString(seedIndex);
                            DriverPathList = GetData.GetInfPathsList(Program.dirName);

                            string line = XMLReader.FromINFIndex(infListCount, Program.InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileName(line);
                            GetData.CreateIfMissing(Program.resultsLogDir);

                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            switch (isCapsule)
                            {
                                case true:
                                    Logger.Comment("this is firmware treat it as such and reboot or rollback\reboot...");
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                    break;
                                //case bool surfaceInName when infName.Contains("Surface").Equals(true):
                                //    Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                //    infListCount--;
                                //    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                //    break;
                                default:
                                    Logger.Comment("this is NOT firmware check for reboot afer installed");
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                    break;
                            }
                        }
                        RebootAndContinue.RebootCmd(true);
                    }
                    
                    else
                    {
                        // straight through the list 1-# accounting for other parameters below
                        // groupFirmware or not as well as user choice first
                        string testIsStartChoice = GetData.GetTestFirst(Program.InputTestFilePath);
                        if (testIsStartChoice.Equals("none") & (Directory.EnumerateDirectories(Program.dirName, "uefi")).Equals(true))
                        {
                            XMLWriter.SetTestFirst("uefi");
                        }
                        else if (testIsStartChoice.Equals("none") & (Directory.EnumerateDirectories(Program.dirName, "sam")).Equals(true))
                        {
                            XMLWriter.SetTestFirst("sam");
                        }
                        else { testIsStartChoice = "none"; }
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
                                // if this is set to true this will get the firmware in the list to install
                                // install each one and reboot at the end rather than rebooting for each install
                                // replicating more of what a user might see at home when getting more than one
                                // firmware update from WU 
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

                                        switch (isInstalled)
                                        {
                                            case true:
                                                string infFileContent = File.ReadAllText(groupedFirmware).ToUpper();
                                                string infName = Path.GetFileNameWithoutExtension(groupedFirmware);
                                                SafeNativeMethods.RollbackInstall(seedIndex, groupedFirmware, infName, infFileContent, hardwareID, rebootRequired = true, Program.InputTestFilePath);
                                                break;

                                            case false:
                                                string groupedFirmwareDIR = Path.GetDirectoryName(groupedFirmware);
                                                string installArgs = " /C /A /Q /SE /F /PATH " + groupedFirmwareDIR;
                                                SafeNativeMethods.Install_Inf(groupedFirmware, Program.installer, installArgs, seedIndex);
                                                break;
                                        }
                                    }
                                    capListCount--;
                                }
                                RebootAndContinue.RebootCmd(true);
                            }
                        }

                        executionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);
                            if (index.Equals(null)) { continue; }
                            GetData.CreateIfMissing(Program.resultsLogDir);

                            string line = XMLReader.FromINFIndex(infListCount, Program.InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileNameWithoutExtension(line);
                            testIsStartChoice = GetData.GetTestFirst(Program.InputTestFilePath);
                            string testInfName = infName.ToLower();
                            infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                            //infIndexListString = File.ReadAllText(Program.seedFilePath);

                            // the tool will skip to here if none of the other above are met
                            // then looking for a choice set to install first from the list
                            // only works when random is set to False
                            if (line.Contains(testIsStartChoice.ToLower()))
                            {
                                Logger.Comment("This is the start first choice : " + line);
                                bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                                switch (isCapsule)
                                {
                                    case true:
                                        infListCount--;
                                        Logger.Comment("re-add the reg key to start post reboot...");
                                        CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                        break;

                                    //case bool surfaceInName when infName.Contains("Surface").Equals(true):
                                    //    Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                    //    infListCount--;
                                    //    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                    //    break;

                                    default:
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("NOT MATCHING to the startChoice");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        infListCount--;
                                        CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                        break;
                                }
                            }
                        }

                        infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);

                            string indexString = Convert.ToString(index);
                            if (index.Equals(null)) { continue; }

                            string line = XMLReader.FromINFIndex(infListCount, Program.InputTestFilePath, seedIndex, executionCount).ToLower();
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            string infName = Path.GetFileName(line);
                            testIsStartChoice = GetData.GetTestFirst(Program.InputTestFilePath);

                            switch (isCapsule)
                            {
                                case true:
                                    Logger.Comment("this is firmware and will need to reboot...");
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                    break;

                                //case bool surfaceInName when infName.Contains("Surface").Equals(true):
                                //    Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                //    infListCount--;
                                //    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                //    break;

                                default:
                                    Logger.Comment("THIS IS NOT FIRMWARE...");
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, Program.InputTestFilePath);
                                    break;
                            }
                        }
                        RebootAndContinue.RebootCmd(true);
                    }
                    executionCount--;
                    Console.ForegroundColor = ConsoleColor.Cyan;
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

        internal static void DoThisIfFailedPostReboot()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-----------------------------------------.");
            Console.WriteLine("The lastInstalled INF seems to have failed");
            Console.WriteLine("-----------------------------------------.");
            Console.ForegroundColor = ConsoleColor.White;
            ParseForResults.ParseFromdpinstLog(@"C:\Windows\DPINST.LOG");
            File.Create(Program.dirName + @"\DONE.TXT");
            CheckWhatInstalled.CheckInstalledCSV();
            Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
            Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
            Utilities.CopyFile(Program.InputTestFilePathBAK, Program.resultsLogDir + @"\DriverCapsuleStress.xml.BAK");
            Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
            Thread.Sleep(5000);
            Console.WriteLine("Everything should be completed including copying logs to the Reults folder on the desktop");
            // is the WTT service stopped.
            StartStopServices.StartService("wttsvc");
            Logger.FunctionLeave();
            Environment.Exit(13);
        }
    }
}