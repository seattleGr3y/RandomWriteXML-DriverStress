using Microsoft.HWSW.Test.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RandomWriteXML
{
    class SafeNativeMethods
    {
        internal static bool rebootRequired;
        //internal static Process process = new Process();
        enum DriverInstallationExitCode { NotInstalled = 80, RebootRequired = 40, RebootNotRequired = 1, CopiedPackageToDriverStoreButNotInstalled = 100, SuccessfullyUninstalled = 0 };
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
        internal static bool Install_Inf(string line, string installer, string installArgs)
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
            int rebootRquiredExitCode = Convert.ToInt32(DriverInstallationExitCode.RebootRequired);
            int rebootNotRquiredExitCode = Convert.ToInt32(DriverInstallationExitCode.RebootNotRequired);
            int updatedInstallation = Convert.ToInt32(DriverInstallationExitCode.CopiedPackageToDriverStoreButNotInstalled);
            int notInstalledExitCode = Convert.ToInt32(DriverInstallationExitCode.NotInstalled);
            int InstalledExitCode = Convert.ToInt32(DriverInstallationExitCode.SuccessfullyUninstalled);
            int strLen = line.LastIndexOf(@"\");
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

                // sets a timeout period to wait so the program doesn't get stuck waiting endlessly
                bool notTimeOut = process.WaitForExit(TimeOut);
                if (notTimeOut == false)
                {
                    failureCause = "Driver installation Timed-Out";
                    errorCodeMessage = string.Format(line + " Driver installation Timed-Out");
                    Logger.Comment(line + " Driver installation Failed due to  Time-Out::: " + errorCodeMessage, expectedDriverVersion, DateTime.Now.ToString("MM/dd/yyyy H:mm:ss:fff"), beforeDriverStatus, afterDriverStatus, deviceRuning, failureCause);
                }
                //Add metadata to exit code
                if (installationExitCode.Equals(rebootRquiredExitCode))
                {
                    GetData.GetExitCode(rebootRquiredExitCode.ToString(), stdOutput, errorMessage);
                }
                else if (installationExitCode.Equals(0x490))
                {
                    GetData.GetExitCode(installationExitCode.ToString(), stdOutput, errorMessage);
                    Logger.Comment("got exit code 49, try to re-install : " + line);
                    installArgs = " /C /A /Q /SE /F /PATH " + line;
                    Install_Inf(line, installer, installArgs);
                    rebootRequired = true;
                }
                else if (errorMessage.Contains("0x490"))
                {
                    GetData.GetExitCode(installationExitCode.ToString(), stdOutput, errorMessage);
                    Logger.Comment("got exit code 49, try to re-install : " + line);
                    installArgs = " /C /A /Q /SE /F /PATH " + line;
                    Install_Inf(line, installer, installArgs);
                    rebootRequired = true;
                }
                else if (installationExitCode.Equals(0xB7))
                {
                    GetData.GetExitCode(installationExitCode.ToString(), stdOutput, errorMessage);
                    installArgs = " /C /U " + line + " /Q /D";
                    Install_Inf(line, installer, installArgs);
                    rebootRequired = true;
                }
                else if (installationExitCode.Equals(rebootNotRquiredExitCode))
                {
                    GetData.GetExitCode(rebootNotRquiredExitCode.ToString(), stdOutput, errorMessage);
                }
                else if (installationExitCode.Equals(updatedInstallation))
                {
                    GetData.GetExitCode(updatedInstallation.ToString(), stdOutput, errorMessage);
                }
                else if (installationExitCode.Equals(notInstalledExitCode))
                {
                    GetData.GetExitCode(notInstalledExitCode.ToString(), stdOutput, errorMessage);
                    rebootRequired = true;
                }
                if (exitCode.Equals(1))
                {
                    rebootRequired = false;
                }
                else
                {
                    rebootRequired = true;
                }
                process.Dispose();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            Thread.Sleep(1000);
            Logger.Comment("this is the rebootrequired returned : " + rebootRequired);
            Logger.FunctionLeave();
            return rebootRequired;
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
        internal static void InstallUninstallCall(bool rebootRequired, string infName, string line, string installer, string InputTestFilePath, string hardwareID)
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
                    Thread.Sleep(1000);
                    Logger.Comment("rebootRequired is showing as : " + rebootRequired);
                    XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                    installArgs = " /C /U " + line + " /Q /D";
                    Install_Inf(line, installer, installArgs);
                    Logger.Comment("Operation should be complete: " + line);

                    if (rebootRequired.Equals(true))
                    {
                        //XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                        RebootAndContinue.RebootCmd(true);
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                    Logger.Comment("rebootRequired is showing as : " + rebootRequired);
                    XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                    installArgs = " /C /A /Q /SE /F /PATH " + infPath;
                    Install_Inf(line, installer, installArgs);
                    Logger.Comment("Operation should be complete: " + line);

                    if (rebootRequired.Equals(true))
                    {
                        //XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
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
        #region NO LONGER IN USE AT THIS TIME
        ///// <summary>
        ///// this is called when installing firmware to perform the extra actions needed such as regkey writes and reboot
        ///// </summary>
        ///// <param name="infName"></param>
        ///// <param name="infFileContent"></param>
        ///// <param name="hardwareID"></param>
        ///// <param name="rebootRequired"></param>
        ///// <param name="line"></param>
        ///// <param name="installer"></param>
        ///// <param name="installArgs"></param>
        ///// <param name="InputTestFilePath"></param>
        //internal static void FirmwareInstall(string infName, string infFileContent, string hardwareID, string line, string installer, string installArgs, string InputTestFilePath)
        //{
        //    try
        //    {
        //        Logger.FunctionEnter();
        //        RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired.Equals(true));
        //        Logger.Comment("installArgs from FirmwareInstall : " + installArgs);
        //        XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
        //        InstallUninstallCall(rebootRequired = true, infName, line, installer, InputTestFilePath, hardwareID);
        //        Logger.FunctionLeave();
        //    }
        //    catch (Exception ex)
        //    {
        //        GetData.GetExceptionMessage(ex);
        //    }
        //}
        #endregion

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
        internal static void RollbackInstall(string line, string infName, string infFileContent, string hardwareID, bool rebootRequired, string rollbackLine, string installer, string InputTestFilePath)
        {
            Logger.FunctionEnter();
            string installArgs;
            try
            {
                string rbInfName = Path.GetFileNameWithoutExtension(line);
                string rollbackINFnameDIR = @"\" + rbInfName;
                string rollBackDIR = rollbackLine + rollbackINFnameDIR;
                XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                installArgs = " /C /U " + line + " /Q /D";
                Install_Inf(line, installer, installArgs);
                Logger.Comment("Uninstall Operation should be complete: " + line);
                Thread.Sleep(5000);
                installArgs = " /C /A /Q /SE /F /PATH " + rollBackDIR;
                Logger.Comment("start rollback...");
                RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired.Equals(true));
                Thread.Sleep(5000);
                Install_Inf(line, installer, installArgs);
                Thread.Sleep(5000);
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
