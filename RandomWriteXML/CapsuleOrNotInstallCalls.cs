using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        internal static void IsNotCapsule(bool stopOnFail, int seedIndex, string infIndexListString, int? driverPathListCount, string infName, List<string> DriverPathList, string line, string InputTestFilePathBAK, string installer, int executionCount, string dirName, string startChoice, string rollbackLine, string InputTestFilePath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("seedIndex from IsNotCapsule : " + seedIndex);
            DriverStressInit.UpdateXML(seedIndex);

            bool rebootRequired = false;
            Logger.FunctionEnter();
            string infFileContent = File.ReadAllText(line).ToUpper();
            string infDir = Path.GetDirectoryName(line);

            string hardwareID = GetData.FirmwareInstallGetHID(line);
            driverPathListCount--;

            Console.WriteLine("does the xml look correct now?");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.ReadKey();
            Logger.Comment("this will now install : " + infName);
            int infListCount = DriverPathList.Count;
            XMLReader.GetDriversPath(InputTestFilePath, executionCount, infListCount);
            SafeNativeMethods.InstallUninstallCall(stopOnFail, rebootRequired, infName, line, installer, InputTestFilePath, hardwareID);
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
        internal static void IfIsCapsule(bool stopOnFail, int seedIndex, string infIndexListString, int? driverPathListCount, string infName, List<string> DriverPathList, string line, string InputTestFilePathBAK, string installer, int executionCount, string dirName, string startChoice, string rollbackLine, string InputTestFilePath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("seedIndex from IfIsCapsule : " + seedIndex);
            DriverStressInit.UpdateXML(seedIndex);

            bool needRollBack = true;
            bool rebootRequired = true;
            Logger.FunctionEnter();
            string expectedDriverVersion = GetData.GetDriverVersion(line);
            Logger.Comment("IfIsCapsule From RegCheck before IF : " + expectedDriverVersion);
            string infFileContent = File.ReadAllText(line).ToUpper();
            string infDir = Path.GetDirectoryName(line);
            Logger.Comment("IfIsCapsule isCapsule infName " + infName);
            //Directory.CreateDirectory(rollbackLine);
            string infNameToTest = Path.GetFileNameWithoutExtension(line);
            string expectedDriverDate = GetData.GetDriverDate(line);
            string infPath = Path.GetDirectoryName(line);

            string friendlyDriverName = XMLReader.GetFriendlyDriverName(InputTestFilePath, line);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("friendlyDriverName from IfIsCapsule : " + friendlyDriverName);
            Console.WriteLine("infNameToTest from IfIsCapsule : " + infNameToTest);
            Console.WriteLine("expectedDriverVersion from IfIsCapsule : " + expectedDriverVersion);
            Console.WriteLine("expectedDriverDate from IfIsCapsule : " + expectedDriverDate);
            Console.ForegroundColor = ConsoleColor.White;
            //Console.ReadKey();
            bool isInstalled = CheckWhatInstalled.CheckInstalled(friendlyDriverName, infNameToTest, expectedDriverVersion, expectedDriverDate);

            string hardwareID = GetData.FirmwareInstallGetHID(line);

            if (isInstalled)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("THIS VERSION IS CURRENTLY INSTALLED ROLLBACK NOW....");
                Console.WriteLine("----------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                GetData.IfWillNeedRollBack(line, needRollBack, infName);
                driverPathListCount--;
                Logger.Comment("this will now begin to ROLLBACK : " + infName);
                int infListCount = DriverPathList.Count;
                XMLReader.GetDriversPath(InputTestFilePath, executionCount, infListCount);
                SafeNativeMethods.RollbackInstall(stopOnFail, line, infName, infFileContent, hardwareID, rebootRequired = true, rollbackLine, installer, InputTestFilePath);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("THIS VERSION WAS NOT INSTALLED YET INSTALLING NOW...");
                Console.WriteLine("----------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;

                hardwareID = GetData.FirmwareInstallGetHID(line);
                string installArgs = " /C /A /Q /SE /F /PATH " + infDir;
                Logger.Comment("IfIsCapsule installArgsChoice  is FALSE installArgs " + installArgs);
                driverPathListCount--;
                Logger.Comment("this will now Install : " + infName);
                installArgs = " /C /A /Q /SE /F /PATH " + infPath;
                SafeNativeMethods.Install_Inf(stopOnFail, line, installer, installArgs);
                RegCheck.CreatePolicyRegKeyAndSetValue(hardwareID, rebootRequired);
                Logger.Comment("installArgs from FirmwareInstall : " + installArgs);
                int infListCount = DriverPathList.Count;
                XMLReader.GetDriversPath(InputTestFilePath, executionCount, infListCount);
                RebootAndContinue.RebootCmd(true);
            }
            Logger.FunctionLeave();
        }
    }
}
