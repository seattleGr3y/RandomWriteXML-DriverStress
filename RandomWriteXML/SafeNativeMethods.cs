using Microsoft.HWSW.Test.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RandomWriteXML
{
    class SafeNativeMethods
    {
        //internal static Process process = new Process();
        //enum DriverInstallationExitCode { ElementNotFound = 490, DeviceCanNotStart = 100, NotInstalled = 800, RebootRequired = 400, SuccessfullyUninstalled = 000 };
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
        internal static void Install_Inf(bool stopOnFail, string line, string installer, string installArgs)
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
            string DeviceCanNotStartExitCode = "0x100";
            string NotInstalledExitCode = "0x800";
            string ElementNotFoundExitCode = "0x480";
            string NotExistsExitCode = "0xB7";

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
                int exitCode = process.ExitCode;
                string exitCodeInHex = ConvertExitCodeToHex(exitCode);
                string stdOutput = process.StandardOutput.ReadToEnd();
                installationExitCode = "0x" + exitCodeInHex;

                if (stopOnFail)
                {
                    // sets a timeout period to wait so the program doesn't get stuck waiting endlessly
                    bool notTimeOut = process.WaitForExit(TimeOut);
                    if (notTimeOut == false)
                    {
                        failureCause = "Driver installation Timed-Out";
                        errorCodeMessage = string.Format(line + " Driver installation Timed-Out");
                        Logger.Comment(line + " Driver installation Failed due to  Time-Out::: " + errorCodeMessage, expectedDriverVersion, DateTime.Now.ToString("MM/dd/yyyy H:mm:ss:fff"), beforeDriverStatus, afterDriverStatus, deviceRuning, failureCause);

                        Logger.Comment("There was a failure please check the logs...");
                        Logger.Comment("dptinst.exe timed out during install");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There was a failure please check the logs...");
                        Console.WriteLine("dptinst.exe timed out during install");
                        Console.ForegroundColor = ConsoleColor.White;
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ReadKey();
                        Environment.Exit(0);

                    }
                    //Add metadata to exit code
                    else if (installationExitCode.Equals(ElementNotFoundExitCode))
                    {
                        Logger.Comment("There was a failure please check the logs...");
                        Logger.Comment("dptinst.exe threw error code 49, element not found");
                        Logger.Comment("possibly trying to uninstall a driver that is not yet installed");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There was a failure please check the logs...");
                        Console.WriteLine("dptinst.exe threw error code 49, element not found");
                        Console.WriteLine("possibly trying to uninstall a driver that is not yet installed");
                        Console.ForegroundColor = ConsoleColor.White;
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ReadKey();
                        Environment.Exit(0);

                    }
                    else if (installationExitCode.Equals(NotInstalledExitCode))
                    {
                        Logger.Comment("There was a failure please check the logs...");
                        Logger.Comment("failed to install");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There was a failure please check the logs...");
                        Console.WriteLine("failed to install");
                        Console.ForegroundColor = ConsoleColor.White;
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ReadKey();
                        Environment.Exit(0);

                    }
                    else if (installationExitCode.Equals(NotExistsExitCode))
                    {
                        Logger.Comment("There was a failure please check the logs...");
                        Logger.Comment("dptinst.exe threw error Not Exists Exit Code = 0xB7");
                        Logger.Comment("no device exists for this driver");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There was a failure please check the logs...");
                        Console.WriteLine("dptinst.exe threw error Not Exists Exit Code = 0xB7");
                        Console.WriteLine("no device exists for this driver");
                        Console.ForegroundColor = ConsoleColor.White;
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ReadKey();
                        Environment.Exit(0);

                    }
                    else if (installationExitCode.Equals(DeviceCanNotStartExitCode))
                    {
                        Logger.Comment("There was a failure please check the logs...");
                        Logger.Comment("dptinst.exe threw error code 10, Device Can Not Start");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There was a failure please check the logs...");
                        Console.WriteLine("dptinst.exe threw error code 10, Device Can Not Start");
                        Console.ForegroundColor = ConsoleColor.White;
                        Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                        Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                        Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
                        Console.ReadKey();
                        Environment.Exit(0);

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
        internal static void InstallUninstallCall(bool stopOnFail, bool rebootRequired, string infName, string line, string installer, string InputTestFilePath, string hardwareID)
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
                bool isInstalled = CheckWhatInstalled.CheckInstalled(friendlyDriverName, infNameToTest, expectedDriverVersion, expectedDriverDate);
                if (isInstalled)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("THIS VERSION IS INSTALLED NOW UN-INSTALLING NOW...");
                    Console.WriteLine(infName);
                    Console.WriteLine("----------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;
                    Thread.Sleep(500);
                    Logger.Comment("rebootRequired is showing as : " + rebootRequired);
                    //XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                    installArgs = " /C /U " + line + " /Q /D";
                    Install_Inf(stopOnFail, line, installer, installArgs);
                    Logger.Comment("Operation should be complete: " + line);
                    Thread.Sleep(500);

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
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("THIS VERSION WAS NOT INSTALLED YET INSTALLING NOW...");
                    Console.WriteLine(infName);
                    Console.WriteLine("----------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;
                    Thread.Sleep(500);
                    Logger.Comment("rebootRequired is showing as : " + rebootRequired);
                    installArgs = " /C /A /Q /SE /F /PATH " + infPath;
                    Install_Inf(stopOnFail, line, installer, installArgs);
                    Logger.Comment("Operation should be complete: " + line);
                    Thread.Sleep(2000);

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
        internal static void RollbackInstall(bool stopOnFail, string line, string infName, string infFileContent, string hardwareID, bool rebootRequired, string rollbackLine, string installer, string InputTestFilePath)
        {
            Logger.FunctionEnter();
            string installArgs;
            try
            {
                RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired.Equals(true));
                string rbInfName = Path.GetFileNameWithoutExtension(line);
                string rollbackINFnameDIR = @"\" + rbInfName;
                string rollBackDIR = rollbackLine + rollbackINFnameDIR;
                //XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                installArgs = " /C /U " + line + " /Q /D";
                Install_Inf(stopOnFail, line, installer, installArgs);
                Logger.Comment("Uninstall Operation should be complete: " + line);
                Thread.Sleep(500);
                installArgs = " /C /A /Q /SE /F /PATH " + rollBackDIR;
                Logger.Comment("start rollback...");
                Thread.Sleep(1000);
                Install_Inf(stopOnFail, line, installer, installArgs);
                Thread.Sleep(1000);
                RebootAndContinue.RebootCmd(true);
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
        }
    }
}
