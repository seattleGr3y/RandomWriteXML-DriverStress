using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace DriverCapsuleStressTool
{
    class CapsuleOrNotInstallCalls
    {
        /// <summary>
        /// looking for non firmware drivers to install\uninstall 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="InputTestFilePathBAK"></param>
        /// <param name="installer"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="startChoice"></param>
        /// <param name="rollbackLine"></param>
        /// <param name="InputTestFilePath"></param>
        internal static void IsNotCapsule(int seedIndex, string infIndexListString, int? driverPathListCount, string infName, List<string> DriverPathList, string line, string InputTestFilePathBAK, string installer, int executionCount, string dirName, string startChoice, string rollbackLine, string InputTestFilePath)
        {
            try
            {
                bool rebootRequired = false;
                Logger.FunctionEnter();
                string infFileContent = File.ReadAllText(line).ToUpper();
                string infDir = Path.GetDirectoryName(line);
                string hardwareID = GetData.FirmwareInstallGetHID(line);
                driverPathListCount--;
                Logger.Comment("this will now install infName : " + infName);
                Logger.Comment("this will now install hardwareID : " + hardwareID);
                SafeNativeMethods.InstallUninstallCall(seedIndex, rebootRequired, infName, line, installer, InputTestFilePath);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// method to install firmware specifically
        /// </summary>
        /// <param name="line"></param>
        /// <param name="InputTestFilePathBAK"></param>
        /// <param name="installer"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="startChoice"></param>
        /// <param name="rollbackLine"></param>
        /// <param name="InputTestFilePath"></param>
        internal static void IfIsCapsule(int seedIndex, string infIndexListString, int? driverPathListCount, string infName, List<string> DriverPathList, string line, string InputTestFilePathBAK, string installer, int executionCount, string dirName, string startChoice, string rollbackLine, string InputTestFilePath)
        {
            try
            {
                bool needRollBack = true;
                bool rebootRequired = true;
                Logger.FunctionEnter();
                string infFileContent = File.ReadAllText(line).ToUpper();
                string infDir = Path.GetDirectoryName(line);
                Logger.Comment("IfIsCapsule isCapsule infName " + infName);
                string expectedDriverVersion = GetData.GetDriverVersion(line);
                Logger.Comment("IfIsCapsule From RegCheck before IF : " + expectedDriverVersion);
                string infNameToTest = Path.GetFileNameWithoutExtension(line);
                string expectedDriverDate = GetData.GetDriverDate(line);
                string infPath = Path.GetDirectoryName(line);
                int infListCount = XMLReader.GetInfsPathListCount(Program.InputTestFilePathBAK);

                string friendlyDriverName = XMLReader.GetFriendlyDriverName(InputTestFilePath, line);
                string hardwareID = GetData.FirmwareInstallGetHID(line);
                bool isInstalled = CheckWhatInstalled.CheckInstalled(line, hardwareID, friendlyDriverName, infNameToTest, expectedDriverVersion, expectedDriverDate);
                
                if (isInstalled)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("-------------------------------------------------------------");
                    Console.WriteLine("THIS FIRMWARE VERSION IS CURRENTLY INSTALLED ROLLBACK NOW....");
                    Console.WriteLine("-------------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;
                    GetData.IfWillNeedRollBack(line, needRollBack, infName);
                    driverPathListCount--;
                    Logger.Comment("this will now begin to ROLLBACK : " + infName);
                    SafeNativeMethods.RollbackInstall(seedIndex, line, infName, infFileContent, hardwareID, rebootRequired = true, InputTestFilePath);
                    RebootAndContinue.RebootCmd(true);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("-------------------------------------------------------------");
                    Console.WriteLine("THIS FIRMWARE VERSION WAS NOT INSTALLED YET INSTALLING NOW...");
                    Console.WriteLine("-------------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.White;

                    hardwareID = GetData.FirmwareInstallGetHID(line);
                    string installArgs = " /C /A /Q /SE /F /PATH " + infDir;
                    Logger.Comment("IfIsCapsule installArgsChoice  is FALSE installArgs " + installArgs);
                    driverPathListCount--;
                    Logger.Comment("this will now Install : " + infName);
                    installArgs = " /C /A /Q /SE /F /PATH " + infDir;
                    
                    SafeNativeMethods.Install_Inf(line, installer, installArgs, seedIndex);
                    RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired);
                    Logger.Comment("installArgs from FirmwareInstall : " + installArgs);
                    bool isInstalledRegCheck = GetDataFromReg.CheckRegCapsuleIsInstalled(infName, hardwareID, expectedDriverVersion, line);
                    if (isInstalledRegCheck.Equals(true))
                    {
                        RebootAndContinue.RebootCmd(true);
                    }
                    else
                    {
                        bool stopOnError = XMLReader.GetStopOnError(Program.InputTestFilePathBAK);
                        if (stopOnError)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("-------------------------------------------------------------");
                            Console.WriteLine("THIS FAILED TO INSTALL ACCORDING TO THE REGISTRY...");
                            Console.WriteLine("-------------------------------------------------------------");
                            Console.ForegroundColor = ConsoleColor.White;

                            Console.ReadKey();
                        }
                        RebootAndContinue.RebootCmd(true);
                    }
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
