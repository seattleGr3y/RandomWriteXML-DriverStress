using Microsoft.HWSW.Test.Utilities;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DriverCapsuleStressTool
{
    class Program
    {
        internal static string dirName = Environment.CurrentDirectory;
        internal static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        internal static string InputTestFile = @"\DriverCapsuleStress.xml";
        internal static string InputTestFilePath = dirName + InputTestFile;
        internal static string InputTestFilePathBAK = dirName + @"\DriverCapsuleStress.xml.BAK";
        internal static string seedFilePath = dirName + @"\SEED.txt";
        internal static string stringList = "";
        internal static string startChoice = "uefi";
        internal static string supportFolderLOC = dirName;
        internal static string randomizeList;
        internal static string groupFirmwareSTR = "false";
        internal static string stopOnErrorSTR = "false";
        internal static string installer = dirName + @"\dpinst.exe";
        internal static string resultsLogDir = desktopPath + @"\Results";
        internal static string dpinstLog = resultsLogDir + @"\DPINST.LOG";
        internal static string lastInstalled = dirName + @"\LastInstalled.txt";
        internal static string rollBackDir = @"\Rollbacks";
        internal static string rollbackLine = (dirName + rollBackDir).ToLower();
        internal static string stressAppPath = dirName + @"\DriverCapsuleStressTool.exe";
        internal static string reStartBAT = dirName + @"\reStart.bat";
        internal static string loopCount;
        internal static string[] args = null;
        [DllImport("kernel32.dll", ExactSpelling = true)]

        private static extern IntPtr GetConsoleWindow();
        private static readonly IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;

        /// <summary>
        /// attempt to move the devMgr window over to the left after it opens
        /// so it is easier to monitor when running locally if need be
        /// currently not moving though
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="bRepaint"></param>
        /// <returns></returns>

        internal static void Main(string[] args)
        {
            bool show_help = false;

            List<string> extra = new List<string>();
            ShowWindow(ThisConsole, MAXIMIZE);

            try
            {

                Console.WriteLine("this is the dirName now : " + dirName);
                if (!File.Exists(reStartBAT))
                {
                    File.WriteAllText(reStartBAT, "cd " + dirName + Environment.NewLine + stressAppPath);
                }

                string stressLog = "DriverCapsuleStressLog.txt";
                Logger.AppendToFile = true;
                Logger.LogFileName = stressLog;
                Logger.LogDirName = dirName;

                if (!File.Exists(stressLog))
                {
                    Logger.AddLogFile(stressLog);
                }

                //Logger.LogAll();

                if (!File.Exists(InputTestFilePath))
                {

                    var p = new OptionSet() {
                        { "r|randomizeList", "True or False - randomize the execution of the INFs",
                          v => randomizeList = v },
                        { "i|loopCount", "the number of {TIMES} to stress drivers\\capsules. this must be an integer.",
                          v => loopCount = v },
                        { "s|startChoice", "Choose driver\\capsule to install first(default will be UEFI",
                          v => startChoice = v },
                        { "e|stopOnErrorSTR", "Stop testing if there is an error or failure and collect logs",
                          v => stopOnErrorSTR = v },
                        { "g|groupFirmwareSTR", "Install all firmware then reboot rather than install\reboot for each",
                          v => groupFirmwareSTR = v },
                        { "h|help",  "show this message and exit",
                          v => show_help = v != null },
                        };

                    if (show_help)
                    {
                        ShowHelp(p);
                        return;
                    }

                    extra = p.Parse(args);
                    if (extra.Count > 0)
                    {
                        CreateListOrder.RandomizeList(args[1], args[3], args[5], args[7], args[9]);
                    }
                    else
                    {
                        ShowHelp(p);
                        return;
                    }
                }

                else
                {
                    XDocument xdoc = XDocument.Load(InputTestFilePath);
                    string infIndexListString = XMLReader.GetSeed(InputTestFilePathBAK);
                    int executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                    supportFolderLOC = xdoc.XPathSelectElement("/Tests/TestChoices/SupportFolder").Value.ToString();
                    Array infListFromXML = xdoc.XPathSelectElements("/Tests/InfDirectories/InfDir").ToArray();
                    bool randomize = GetData.GetRandomChoice(InputTestFilePath);
                    ExecuteFromList.ExecuteTheList(randomize, executionCount, startChoice);
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("");
            Console.WriteLine("------------------------------");
            Console.WriteLine("Usage: ");
            Console.WriteLine("DriverCapsuleStressTool");
            Console.WriteLine("------------------------------");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}