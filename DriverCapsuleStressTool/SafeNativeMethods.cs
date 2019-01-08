using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
            string notInstalledExitCode = "0x800";
            string ElementNotFoundCode = "0x490";
            string DeviceCanNotStartCode = "0x10";
            //string NoMatchingDevice = "0xB7";
            #endregion

            try
            {
                // starts the installation and checks for exit code
                //
                Logger.Comment("...trying to install now");
                Logger.Comment("=========================");
                Logger.Comment(line);
                Logger.Comment(installer + installArgs);

                if (installArgs.Contains("/u /c"))
                {
                    XMLWriter.SetUnInstallCount(Program.InputTestFilePathBAK, seedIndex);
                }
                else
                {
                    XMLWriter.SetInstallCount(Program.InputTestFilePathBAK, seedIndex);
                }

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
                if (Program.stopOnError)
                {
                    if (notTimeOut == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        failureCause = "Driver installation Timed-Out";
                        errorCodeMessage = string.Format(line + " Driver installation Timed-Out");
                        Logger.Comment(line + " Driver installation Failed due to  Time-Out::: " + errorCodeMessage, expectedDriverVersion, DateTime.Now.ToString("MM/dd/yyyy H:mm:ss:fff"), beforeDriverStatus, afterDriverStatus, deviceRuning, failureCause);
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        Environment.Exit(13);
                    }
                    //Add metadata to exit code
                    else if (installationExitCode.Equals(ElementNotFoundCode))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        GetData.GetExitCode(installationExitCode, stdOutput, errorMessage);
                        Logger.Comment("got exit code 49, try to re-install : " + line);
                        Logger.Comment("Elemnt not found error, trying to uninstall a device that is not currently installed");
                        Console.WriteLine("got exit code 49, try to re-install : " + line);
                        Console.WriteLine("Elemnt not found error, trying to uninstall a device that is not currently installed");
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        Environment.Exit(13);
                    }
                    else if (installationExitCode.Equals(DeviceCanNotStartCode))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Comment("CODE 10 : device cannot start error for " + line);
                        Console.WriteLine("CODE 10 : device cannot start error for " + line);
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        Environment.Exit(13);
                    }
                    else if (installationExitCode.Equals(notInstalledExitCode))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Comment("not installed...what is up...");
                        Console.WriteLine("not installed...what is up...");
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        Environment.Exit(13);
                    }
                }
                process.Dispose();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            Thread.Sleep(1000);
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
                XMLWriter.RemoveXMLElemnt(Program.InputTestFilePath, line, seedIndex);
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
                    //XMLWriter.SetUnInstallCount(Program.InputTestFilePathBAK, seedIndex);
                    Thread.Sleep(1000);
                    installArgs = " /C /U " + line + " /Q /D";
                    Install_Inf(line, installer, installArgs, seedIndex);
                    Logger.Comment("Operation should be complete: " + line);
                    Thread.Sleep(500);

                    string classGUID = GetData.GetClassGUID(line);
                    GetDataFromReg.GetOEMinfNameFromReg(infName, hardwareID, classGUID);

                    if (RegCheck.IsRebootPending())
                    {
                        Logger.Comment("there is a pending reboot...");
                        Thread.Sleep(1000);
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

                    //XMLWriter.SetInstallCount(Program.InputTestFilePathBAK, seedIndex);
                    Thread.Sleep(1000);
                    Logger.Comment("rebootRequired is showing as : " + rebootRequired);
                    installArgs = " /C /A /Q /SE /F /PATH " + infPath;
                    Install_Inf(line, installer, installArgs, seedIndex);
                    
                    //can we check registry simiarly to firmware for PnP drivers?

                    Logger.Comment("Operation should be complete: " + line);

                    if (RegCheck.IsRebootPending())
                    {
                        Logger.Comment("there is a pending reboot...");
                        Thread.Sleep(2000);
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
            try
            {
                XMLWriter.RemoveXMLElemnt(Program.InputTestFilePath, line, seedIndex);
                RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired.Equals(true));
                string rbInfName = Path.GetFileNameWithoutExtension(line);
                string infPath = Path.GetDirectoryName(line);

                fullRollBackDIR = GetData.CheckRollbacksExist(line, infName);
                string fullRollBackDIRdir = Path.GetDirectoryName(fullRollBackDIR);

                installArgs = " /C /U " + line + " /Q /D";
                Install_Inf(line, Program.installer, installArgs, seedIndex);
                Logger.Comment("Uninstall Operation should be complete: " + line);
                Thread.Sleep(500);
                installArgs = " /C /A /Q /SE /F /PATH " + fullRollBackDIRdir; 
                Logger.Comment("start rollback...");
                Thread.Sleep(1000);
                Install_Inf(line, Program.installer, installArgs, seedIndex);
                Thread.Sleep(1000);
                string classGUID = GetData.GetClassGUID(line);
                string expectedDriverVersion = GetData.GetDriverVersion(line);
                GetDataFromReg.GetOEMinfNameFromReg(infName, hardwareID, classGUID);
                bool isInstalledRegCheck = GetDataFromReg.CheckRegIsInstalled(infName, hardwareID, expectedDriverVersion, line);
                
                if (isInstalledRegCheck.Equals(false))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("according to the registry this did not actually install");
                    Console.WriteLine("..even if there are no errors being thrown this needs to be");
                    Console.WriteLine("manually checked...NOW");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
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
