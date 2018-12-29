using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        internal static Random rnd = new Random();
        #endregion

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        /// <summary>
        /// Where most of the work is done to decide what to do then call the methods to do so
        /// </summary>
        /// <param name="installer"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="startChoice"></param>
        internal static void StartStress(string InputTestFilePath, string installer, string dirName, string startChoice, string rollbackLine, int infListCount = 0)
        {
            Process devMgr = new Process();
            devMgr.StartInfo.FileName = @"C:\Windows\System32\mmc.exe";
            devMgr.StartInfo.Arguments = "devmgmt.msc";
            devMgr.Start();
            Thread.Sleep(100);

            if (devMgr.MainWindowTitle.Contains("Device Manager"))
            {
                IntPtr handle = devMgr.MainWindowHandle;
                MoveWindow(handle, 1200, 150, 90, 40, true);
            }

            string stressAppPath = dirName + @"\RandomWriteXML.exe";
            int? driverPathListCount = 0;

            Logger.FunctionEnter();

            if (!File.Exists(Program.dirName + @"\debugEnabled.txt"))
            {
                Logger.Comment("enabled WinDebugMode we must reboot...");
                Thread.Sleep(3000);
                RebootAndContinue.EnableWinDebugMode();
            }
            if (RegCheck.IsRebootPending())
            {
                Logger.Comment("there is a pending reboot...");
                Thread.Sleep(3000);
                RebootAndContinue.RebootCmd(true);
            }
            if (GetData.CheckCrashDumpOccurred())
            {
                Logger.Comment("Looks like we found a crashdump check it out after reboot...");
                Thread.Sleep(3000);
                RebootAndContinue.RebootCmd(true);
            }

            CheckWhatInstalled.CheckInstalledCSV();

            string InputTestFilePathBAK = dirName + @"\StressTestXML.xml.BAK";
            List<string> DriverPathList = new List<string>();

            DriverPathList = GetData.GetInfPathsList(Program.dirName);
            driverPathListCount = DriverPathList.Count;
            infListCount = DriverPathList.Count;
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

            else if (driverPathListCount == 0)
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

        internal static void RewriteXMLContinue(int executionCount, int infListCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("am i really supposed to be rewriting the XML right now?");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
            XMLWriter.DecrementExecutionCount(Program.InputTestFilePathBAK, executionCount);
            Logger.Comment("executionCount after going thru all the loops : " + executionCount);
            File.Delete(Program.InputTestFilePath);
            Utilities.CopyFile(Program.InputTestFilePathBAK, Program.InputTestFilePath);
            var numbers = new List<int>(Enumerable.Range(1, infListCount));

            string infIndexList = string.Empty;
            // remove existing data in startSeed and currentSeed from .BAK file before copy
            if (GetData.GetRandomChoice(Program.InputTestFilePath).Equals(true))
            {
                numbers.Shuffle(infListCount);
                infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                File.WriteAllText(Program.seedFilePath, infIndexList);
                XMLWriter.SaveSeed(Program.InputTestFilePathBAK, infIndexList, infIndexList);
            }

            infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
            File.WriteAllText(Program.seedFilePath, infIndexList);
            XMLWriter.SaveSeed(Program.InputTestFilePathBAK, infIndexList, infIndexList);
            Utilities.CopyFile(Program.InputTestFilePathBAK, Program.InputTestFilePath);
            Logger.Comment("re-add the reg key to start post reboot...");
            Thread.Sleep(3000);
        }

        internal static void UpdateXML(int seedIndex)
        {
            string tempCurrentString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
            string trimCurrentSeed = tempCurrentString.Replace(seedIndex.ToString() + ',', "");
            string currentSeed = trimCurrentSeed.TrimEnd(',').TrimStart(',');
            string StartSeed = XMLReader.GetStartSeed(Program.InputTestFilePathBAK);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("seedIndex from IfIsCapsule : " + seedIndex);
            Console.WriteLine("StartSeed from IfIsCapsule : " + StartSeed);
            Console.WriteLine("currentSeed from IfIsCapsule : " + currentSeed);
            Console.WriteLine("infName from IfIsCapsule : " + currentSeed);
            XMLWriter.SaveCurrentSeed(StartSeed, Program.InputTestFilePathBAK, currentSeed);
            XMLWriter.RemoveXMLElemnt(Program.InputTestFilePath, infName);
            File.WriteAllText(Program.seedFilePath, currentSeed);
        }
    }
}
