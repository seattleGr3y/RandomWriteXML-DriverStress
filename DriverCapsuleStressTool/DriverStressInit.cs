using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DriverCapsuleStressTool
{
    class DriverStressInit
    {
        #region STRINGS AND THINGS
        internal static bool rebootRequired = false;
        internal static string infName = string.Empty;
        internal static string expectedDriverVersion = string.Empty;
        internal static string installArgs = string.Empty;
        internal static bool needRollBack = false;
        internal static Random rnd = new Random();
        #endregion

        /// <summary>
        /// Where most of the work is done to decide what to do then call the methods to do so
        /// </summary>
        /// <param name="installer"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="startChoice"></param>
        internal static void StartStress(string InputTestFilePath, string installer, string dirName, string startChoice, string rollbackLine, int infListCount = 0)
        {
            string stressAppPath = dirName + @"\RandomWriteXML.exe";
            //int? driverPathListCount = 0;

            Logger.FunctionEnter();

            if (!File.Exists(dirName + @"\debugEnabled.txt"))
            {
                Logger.Comment("enabled WinDebugMode we must reboot...");
                Thread.Sleep(1000);
                RebootAndContinue.EnableWinDebugMode();
            }
            if (RegCheck.IsRebootPending())
            {
                Logger.Comment("there is a pending reboot...");
                Thread.Sleep(1000);
                RebootAndContinue.RebootCmd(true);
            }
            if (GetData.CheckCrashDumpOccurred())
            {
                Logger.Comment("Looks like we found a crashdump check it out after reboot...");
                Thread.Sleep(1000);
                RebootAndContinue.RebootCmd(true);
            }

            CheckWhatInstalled.CheckInstalledCSV();

            string InputTestFilePathBAK = dirName + @"\StressTestXML.xml.BAK";
            List<string> DriverPathList = new List<string>();

            DriverPathList = GetData.GetInfPathsList(Program.dirName);
            infListCount = XMLReader.GetInfsPathListCount(Program.InputTestFilePathBAK);
            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");
            int executionCount = XMLReader.GetExecutionCount(InputTestFilePath);

            if (!File.Exists(InputTestFilePathBAK))
            {
                Utilities.CopyFile(InputTestFilePath, InputTestFilePathBAK);
            }

            string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
            if (string.IsNullOrEmpty(infIndexListString))
            {
                executionCount--;
                RewriteXMLContinue(executionCount, infListCount);
            }

            else if (infListCount == 0)
            {
                executionCount--;
                RewriteXMLContinue(executionCount, infListCount);
            }
            else if (DriverPathList.Equals(null))
            {
                executionCount--;
                RewriteXMLContinue(executionCount, infListCount);
            }
            Logger.FunctionLeave();
        }

        /// <summary>
        /// when we have run through the list this will reset the XML so it will start the list
        /// again and ensure the execution count is correct so testing will eventually complete
        /// </summary>
        /// <param name="executionCount"></param>
        /// <param name="infListCount"></param>
        internal static void RewriteXMLContinue(int executionCount, int infListCount)
        {
            XMLWriter.DecrementExecutionCount(Program.InputTestFilePathBAK, executionCount);
            Logger.Comment("executionCount after going thru all the loops : " + executionCount);
            File.Delete(Program.InputTestFilePath);
            var numbers = new List<int>(Enumerable.Range(1, infListCount));

            string infIndexList = string.Empty;
            // remove existing data in startSeed and currentSeed from .BAK file before copy
            if (Program.randomize.Equals(true))
            {
                numbers.Shuffle(infListCount);
                infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                File.WriteAllText(Program.seedFilePath, infIndexList);

                XMLWriter.SaveSeed(Program.InputTestFilePathBAK, infIndexList, infIndexList);
                Utilities.CopyFile(Program.InputTestFilePathBAK, Program.InputTestFilePath);
                Thread.Sleep(1000);
            }
            else
            {
                infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                File.WriteAllText(Program.seedFilePath, infIndexList);

                XMLWriter.SaveSeed(Program.InputTestFilePathBAK, infIndexList, infIndexList);
                Utilities.CopyFile(Program.InputTestFilePathBAK, Program.InputTestFilePath);
                Logger.Comment("re-add the reg key to start post reboot...");
                Thread.Sleep(1000);
            }
        }
    }
}
