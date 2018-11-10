using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace RandomWriteXML
{
    class CapsuleOrNotInstallCalls
    {
        /// <summary>
        /// looking for drivers to install\uninstall that were not the
        /// user choice to install first
        /// </summary>
        /// <param name="line"></param>
        /// <param name="InputTestFilePathBAK"></param>
        /// <param name="installer"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="startChoice"></param>
        /// <param name="rollbackLine"></param>
        /// <param name="InputTestFilePath"></param>
        internal static void IsNotCapsule(int? driverPathListCount, string infName, List<string> DriverPathList, string line, string InputTestFilePathBAK, string installer, int executionCount, string dirName, string startChoice, string rollbackLine, string InputTestFilePath)
        {
            bool rebootRequired = false;
            Logger.FunctionEnter();
            string infFileContent = File.ReadAllText(line).ToUpper();
            string infDir = Path.GetDirectoryName(line);

            string hardwareID = GetData.FirmwareInstallGetHID(line);
            driverPathListCount--;

            Logger.Comment("this will now install : " + infName);
            SafeNativeMethods.InstallUninstallCall(rebootRequired, infName, line, installer, InputTestFilePath, hardwareID);
            Logger.FunctionLeave();
        }

        /// <summary>
        /// method looking for firmware specifically
        /// </summary>
        /// <param name="line"></param>
        /// <param name="InputTestFilePathBAK"></param>
        /// <param name="installer"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="startChoice"></param>
        /// <param name="rollbackLine"></param>
        /// <param name="InputTestFilePath"></param>
        internal static void IfIsCapsule(int? driverPathListCount, string infName, List<string> DriverPathList, string line, string InputTestFilePathBAK, string installer, int executionCount, string dirName, string startChoice, string rollbackLine, string InputTestFilePath)
        {
            bool needRollBack = true;
            bool rebootRequired = true;
            Logger.FunctionEnter();
            string expectedDriverVersion = GetData.GetDriverVersion(line);
            Logger.Comment("IfIsCapsule From RegCheck before IF : " + expectedDriverVersion);
            string infFileContent = File.ReadAllText(line).ToUpper();
            string infDir = Path.GetDirectoryName(line);
            Logger.Comment("IfIsCapsule isCapsule infName " + infName);
            Directory.CreateDirectory(rollbackLine);
            string infNameToTest = Path.GetFileNameWithoutExtension(line);
            string expectedDriverDate = GetData.GetDriverDate(line);
            string infPath = Path.GetDirectoryName(line);

            string friendlyDriverName = XMLReader.GetFriendlyDriverName(InputTestFilePath, line);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("friendlyDriverName from IfIsCapsule : " + friendlyDriverName);
            Console.WriteLine("friendlyDriverName from infNameToTest : " + infNameToTest);
            Console.WriteLine("friendlyDriverName from expectedDriverVersion : " + expectedDriverVersion);
            Console.WriteLine("friendlyDriverName from expectedDriverDate : " + expectedDriverDate);
            Console.ForegroundColor = ConsoleColor.White;
            bool isInstalled = CheckWhatInstalled.CheckInstalled(friendlyDriverName, infNameToTest, expectedDriverVersion, expectedDriverDate);

            string hardwareID = GetData.FirmwareInstallGetHID(line);

            if (isInstalled)
            {
                GetData.IfWillNeedRollBack(line, needRollBack, infName, rollbackLine);
                driverPathListCount--;
                Logger.Comment("this will now begin to ROLLBACK : " + infName);
                SafeNativeMethods.RollbackInstall(line, infName, infFileContent, hardwareID, rebootRequired = true, rollbackLine, installer, InputTestFilePath);
            }
            else
            {
                hardwareID = GetData.FirmwareInstallGetHID(line);
                string installArgs = " /C /A /Q /SE /F /PATH " + infDir;
                Logger.Comment("IfIsCapsule installArgsChoice  is FALSE installArgs " + installArgs);
                driverPathListCount--;
                Logger.Comment("this will now Install : " + infName);
                installArgs = " /C /A /Q /SE /F /PATH " + infPath;
                SafeNativeMethods.Install_Inf(line, installer, installArgs);
                RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired);
                Logger.Comment("installArgs from FirmwareInstall : " + installArgs);
                XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                RebootAndContinue.RebootCmd(true);
            }
            Logger.FunctionLeave();
        }
    }
}
