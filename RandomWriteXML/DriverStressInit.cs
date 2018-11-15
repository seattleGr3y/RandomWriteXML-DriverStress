using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RandomWriteXML
{
    class DriverStressInit
    {
        #region STRINGS AND THINGS
        internal static bool rebootRequired = false;
        internal static string infName = string.Empty;
        internal static string expectedDriverVersion = string.Empty;
        internal static string installArgs = string.Empty;
        internal static bool needRollBack = false;
        //internal static string stressAppPath = @".\DriverStress-2.exe";
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
            int? driverPathListCount = 0;
            Process devMgr = new Process();
            devMgr.StartInfo.FileName = @"C:\Windows\System32\mmc.exe";
            devMgr.StartInfo.Arguments = "devmgmt.msc";
            devMgr.Start();
            Thread.Sleep(100);

            Logger.FunctionEnter();

            if (!File.Exists(Program.dirName + @"\debugEnabled.txt"))
            {
                Logger.Comment("re-add the reg key to start post reboot...");
                Thread.Sleep(3000);
                RebootAndContinue.EnableWinDebugMode();
            }
            if (RegCheck.IsRebootPending())
            {
                Logger.Comment("re-add the reg key to start post reboot...");
                Thread.Sleep(3000);
                RebootAndContinue.RebootCmd(true);
            }
            if (GetData.CheckCrashDumpOccurred())
            {
                Logger.Comment("Looks like we found a crashdump check it out...");
                Thread.Sleep(3000);
                RebootAndContinue.RebootCmd(true);
            }

            CheckWhatInstalled.CheckInstalledCSV();

            string InputTestFilePathBAK = dirName + @"\StressTestXML.xml.BAK";
            List<string> DriverPathList = new List<string>();
            DriverPathList = XMLReader.GetDriversPath(InputTestFilePath);
            driverPathListCount = DriverPathList.Count;
            infListCount = DriverPathList.Count;
            Directory.CreateDirectory(Program.dirName + @"\RESULTS");
            int executionCount = XMLReader.GetExecutionCount(InputTestFilePath);

            if (!File.Exists(InputTestFilePathBAK))
            {
                Utilities.CopyFile(InputTestFilePath, InputTestFilePathBAK);
            }

            string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
            if (infIndexListString.Equals(null))
            {
                SetupToContinue();
            }

            else if (driverPathListCount == 0)
            {
                SetupToContinue();
            }
            else if (DriverPathList.Equals(null))
            {
                SetupToContinue();
            }
            Logger.FunctionLeave();
        }

        internal static void SetupToContinue() 
        {
            executionCount--;
            XMLWriter.DecrementExecutionCount(InputTestFilePathBAK, executionCount);
            string StartSeed = string.Empty;
            string currentSeed = string.Empty;
            // remove existing data in startSeed and currentSeed from .BAK file before copy
            XMLWriter.SaveSeed(InputTestFilePathBAK, StartSeed, currentSeed);

            var numbers = new List<int>(Enumerable.Range(1, infListCount));
            numbers.Shuffle(infListCount);
            string infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
            File.WriteAllText(Program.seedFilePath, infIndexList);

            Utilities.CopyFile(InputTestFilePathBAK, InputTestFilePath);
            Thread.Sleep(3000);
        }
    }
}
