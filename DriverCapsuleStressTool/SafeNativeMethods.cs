using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.HWSW.Test.Utilities;

namespace DriverCapsuleStressTool
{
    class SafeNativeMethods
    {
        //internal static Process process = new Process();
        //No devices found that match driver(s) - code : 0xE000020B
        // DeviceCanNotStart - code : 10
        // No matching device was found - code : 0xB7
        //enum DriverInstallationExitCode { ElementNotFound = 490,  DeviceCanNotStart = 100, NotInstalled = 800, RebootRequired = 400, SuccessfullyUninstalled = 000 };
        internal static string ConvertExitCodeToHex(int exitCode)
        {
            return String.Format("{0:X}", exitCode);
        }

        /// <summary>
        /// This does the driver\capsule installation and checks for the exit code to 
        /// decide what to do next
        /// </summary>
        /// <param name="line"></param>
        /// <param name="installer"></param>
        /// <param name="installArgs"></param>
        internal static void Install_Inf(string line, string installer, string installArgs, int seedIndex)
        {
            Logger.FunctionEnter();
            #region STRINGS AND THINGS
            string failureCause = "None";
            string expectedDriverVersion = "N/A";
            string beforeDriverStatus = "N/A";
            string afterDriverStatus = "N/A";
            bool deviceRuning = true;
            string errorCodeMessage;
            string installationExitCode;
            int TimeOut = 120;
            //string notInstalledExitCode = "0x800";
            string couldNotFindInfToDelete = "0x80000000";
            string ElementNotFoundCode = "0x490";
            string DeviceCanNotStartCode = "0x10";
            string InfFileAlteredCode = "0x80010000";
            string NoMatchingDevice = "0xB7";
            #endregion

            try
            {
                // starts the installation and checks for exit code
                //
                Logger.Comment("...trying to install now");
                Logger.Comment("=========================");
                Logger.Comment(line);
                Logger.Comment(installer + installArgs);
 
                string runCommandinstallArgs = installArgs;
                Process process = Utilities.RunCommand(installer, runCommandinstallArgs, true);

                string errorMessage = process.StandardError.ReadToEnd();
                string stdOutput = process.StandardOutput.ReadToEnd();
                int exitCode = process.ExitCode;
                string exitCodeInHex = ConvertExitCodeToHex(exitCode);
                string tmpInstExitCode = "0x" + exitCodeInHex;
                if (tmpInstExitCode.Equals("0x1"))
                {
                    tmpInstExitCode = "0x11100000";
                }
                else if (tmpInstExitCode.Equals("0x0"))
                {
                    tmpInstExitCode = "0x00000000";
                }
                if (tmpInstExitCode.Length <= 5)
                {
                    installationExitCode = tmpInstExitCode;
                }
                else
                {
                    installationExitCode = tmpInstExitCode.Remove(5);
                }

                Console.WriteLine("installationExitCode : " + installationExitCode);

                // sets a timeout period to wait so the program doesn't get stuck waiting endlessly
                bool notTimeOut = process.WaitForExit(TimeOut);
                bool stopOnError = XMLReader.GetStopOnError(Program.InputTestFilePathBAK);
                if (stopOnError)
                {
                    string lineContains = string.Empty;
                    switch (lineContains)
                    {
                        case string noMatching when stdOutput.Contains(NoMatchingDevice):
                            GetData.GetExitCode(installationExitCode, stdOutput, errorMessage);
                            Logger.Comment("got exit code 0xB7, No Matching Device : " + line);
                            Console.WriteLine("got exit code 0xB7, No Matching Device : " + line);
                            Logger.Comment("Copy the driverstress log and DPINST.LOG to our folder...");
                            Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
                            Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverSDriverCapsuleStressLogtressLog.txt");
                            Console.ReadKey();
                            Environment.Exit(13);
                            break;

                        case string elementNotFound when installationExitCode.Equals(ElementNotFoundCode):
                            GetData.GetExitCode(installationExitCode, stdOutput, errorMessage);
                            Logger.Comment("got exit code 49, try to re-install : " + line);
                            Logger.Comment("Elemnt not found error, trying to uninstall a device that is not currently installed");
                            Console.WriteLine("got exit code 49, try to re-install : " + line);
                            Console.WriteLine("Elemnt not found error, trying to uninstall a device that is not currently installed");
                            Logger.Comment("Copy the driverstress log and DPINST.LOG to our folder...");
                            Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
                            Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
                            Console.ReadKey();
                            Environment.Exit(13);
                            break;

                        case string cannotStart when installationExitCode.Equals(DeviceCanNotStartCode):
                            Logger.Comment("CODE 10 : device cannot start error for " + line);
                            Console.WriteLine("CODE 10 : device cannot start error for " + line);
                            Logger.Comment("Copy the driverstress log and DPINST.LOG to our folder...");
                            Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
                            Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
                            Console.ReadKey();
                            Environment.Exit(13);
                            break;

                        case string couldNotFindInf when installationExitCode.Equals(couldNotFindInfToDelete):
                            Logger.Comment("could Not Find Inf To Delete...what is up...check the logs later...");
                            Console.WriteLine("could Not Find Inf To Delete...what is up...check the logs later...");
                            Logger.Comment("Just move the driverstress log and DPINST.LOG to our folder...");
                            File.Move(@"C:\Windows\DPINST.LOG", Program.dpinstLog + ".ERR");
                            File.Move(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog-ERR.txt");
                            Environment.Exit(13);
                            break;

                        case string InfFileAltered when installationExitCode.Equals(InfFileAlteredCode):
                            Logger.Comment("this was not installed because the inf file has been altered");
                            Console.WriteLine("this was not installed because the inf file has been altered");
                            Logger.Comment("Copy the driverstress log and DPINST.LOG to our folder...");
                            Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
                            Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
                            Environment.Exit(13);
                            break;

                        default:
                            if (notTimeOut == false)
                            {
                                failureCause = "Driver installation Timed-Out";
                                errorCodeMessage = string.Format(line + " Driver installation Timed-Out");
                                Logger.Comment(line + " Driver installation Failed due to  Time-Out::: " + errorCodeMessage, expectedDriverVersion, DateTime.Now.ToString("MM/dd/yyyy H:mm:ss:fff"), beforeDriverStatus, afterDriverStatus, deviceRuning, failureCause);
                                Logger.Comment("Copy the driverstress log and DPINST.LOG to our folder...");
                                Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
                                Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
                                Console.ReadKey();
                                Environment.Exit(13);
                            }
                            break;
                    }
                }
                process.Dispose();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
        }

        /// <summary>
        /// firmware or regular driver will still call this to do the actual installation
        /// </summary>
        /// <param name="infName"></param>
        /// <param name="line"></param>
        /// <param name="installer"></param>
        /// <param name="installArgs"></param>
        /// <param name="InputTestFilePath"></param>
        /// <param name="rebootRequired"></param>
        /// <param name="hardwareID"></param>
        /// SafeNativeMethods.InstallUninstallCall(seedIndex, rebootRequired, infName, line, installer, InputTestFilePath);
        internal static void InstallUninstallCall(int seedIndex, bool rebootRequired, string infName, string line, string installer, string InputTestFilePath)
        {
            try
            {
                Logger.FunctionEnter();
                string expectedDriverVersion = GetData.GetDriverVersion(line);
                string infNameToTest = Path.GetFileNameWithoutExtension(line);
                string expectedDriverDate = GetData.GetDriverDate(line);
                string infPath = Path.GetDirectoryName(line);
                string installArgs;
                string friendlyDriverName = XMLReader.GetFriendlyDriverName(InputTestFilePath, line);
                Console.WriteLine(line + " and " + seedIndex);
                Console.WriteLine("RemoveXMLElement now");
                XMLWriter.RemoveXMLElemnt(Program.InputTestFilePath, line, seedIndex);
                Console.WriteLine("check the xml please..");
                string hardwareID = GetData.FirmwareInstallGetHID(line);
                bool isInstalled = CheckWhatInstalled.CheckInstalled(line, hardwareID, friendlyDriverName, infNameToTest, expectedDriverVersion, expectedDriverDate);
                if (isInstalled)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("----------------------------------------------------------");
                    Console.WriteLine("THIS PnP DRIVER VERSION IS INSTALLED UN-INSTALLING NOW....");
                    Console.WriteLine(infName);
                    Console.WriteLine("----------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;

                    Logger.Comment("rebootRequired is showing as : " + rebootRequired);
                    installArgs = " /C /U " + line + " /Q /D";
                    Install_Inf(line, installer, installArgs, seedIndex);
                    Logger.Comment("Operation should be complete: " + line);

                    string classGUID = GetData.GetClassGUID(line);
                    GetDataFromReg.GetOEMinfNameFromReg(infName, hardwareID, classGUID);

                    if (RegCheck.IsRebootPending())
                    {
                        Logger.Comment("there is a pending reboot...");
                        RebootAndContinue.RebootCmd(true);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine("THIS PnP DRIVER VERSION WAS NOT INSTALLED YET INSTALLING NOW...");
                    Console.WriteLine(infName);
                    Console.WriteLine("---------------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;

                    Logger.Comment("rebootRequired is showing as : " + rebootRequired);
                    installArgs = " /C /A /Q /SE /F /PATH " + infPath;
                    Install_Inf(line, installer, installArgs, seedIndex);
                    string classGUID = GetData.GetClassGUID(line);
                    XMLWriter.SetLastInstalled(line);

                    //can we check registry simiarly to firmware for PnP drivers?
                    Logger.Comment("Operation should be complete: " + line);

                    if (RegCheck.IsRebootPending())
                    {
                        Logger.Comment("there is a pending reboot...");
                        RebootAndContinue.RebootCmd(true);
                    }
                }
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// this is called when installing firmware to perform the extra actions needed such as regkey writes and reboot
        /// </summary>
        /// <param name="infName"></param>
        /// <param name="infFileContent"></param>
        /// <param name="hardwareID"></param>
        /// <param name="rebootRequired"></param>
        /// <param name="rollBackLine"></param>
        /// <param name="installer"></param>
        /// <param name="installArgs"></param>
        /// <param name="InputTestFilePath"></param>
        internal static void RollbackInstall(int seedIndex, string line, string infName, string infFileContent, string hardwareID, bool rebootRequired, string InputTestFilePath)
        {
            Logger.FunctionEnter();
            string installArgs;
            string fullRollBackFile = string.Empty;
            string fullRollBackDIR = string.Empty;
            string fullRollBackDIRdir = string.Empty;
            try
            {
                XMLWriter.RemoveXMLElemnt(Program.InputTestFilePath, line, seedIndex);
                RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired.Equals(true));

                fullRollBackDIR = GetData.CheckRollbacksExist(line, infName);

                if (string.IsNullOrEmpty(fullRollBackDIR))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("should i even be trying to rollback at all???");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    fullRollBackDIRdir = Path.GetDirectoryName(fullRollBackDIR);
                }

                installArgs = " /C /U " + line + " /Q /D";
                string tmpfileName = string.Empty;
                string tmpVersion = GetData.GetDriverVersion(line);
                var binsList = Directory.EnumerateFiles(@"C:\Windows\Firmware\");
                foreach (string binFileToDelete in binsList)
                {
                    tmpfileName = Path.GetFileNameWithoutExtension(binFileToDelete);
                    tmpVersion = tmpVersion.TrimEnd('0').TrimEnd('0').TrimEnd('.');
                    if (Regex.Match(tmpfileName, tmpVersion).Success)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("binFileToDelete exists : " + tmpfileName);
                        File.Delete(tmpfileName);
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                Install_Inf(line, Program.installer, installArgs, seedIndex);
                Logger.Comment("Uninstall Operation should be complete: " + line);
                installArgs = " /C /A /Q /SE /F /PATH " + fullRollBackDIRdir;
                Logger.Comment("start rollback...");
                XMLWriter.SetLastInstalled(fullRollBackDIR);
                Install_Inf(line, Program.installer, installArgs, seedIndex);
                string classGUID = GetData.GetClassGUID(line);
                string expectedDriverVersion = GetData.GetDriverVersion(line);
                GetDataFromReg.GetOEMinfNameFromReg(infName, hardwareID, classGUID);
                if (RegCheck.IsRebootPending())
                {
                    Logger.Comment("there is a pending reboot...");
                    RebootAndContinue.RebootCmd(true);
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
        }
    }
}
