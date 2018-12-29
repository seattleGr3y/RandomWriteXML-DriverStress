using CommandLine.Utility;
using Microsoft.HWSW.Test.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RandomWriteXML
{
    class Program
    {
        internal static string dirName = Environment.CurrentDirectory;        
        internal static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        internal static string InputTestFile = @"\StressTestXML.xml";
        internal static string InputTestFilePath = dirName + InputTestFile;
        internal static string InputTestFilePathBAK = dirName + @"\StressTestXML.xml.BAK";
        internal static string seedFilePath = dirName + @"\SEED.txt";
        internal static string infIndexList = "";
        internal static string stringList = "";
        internal static string startChoice = string.Empty;
        internal static string supportFolderLOC = string.Empty;
        internal static string stopOnFailSTR = string.Empty;
        internal static int executionCount = 0;
        internal static bool randomize = false;
        internal static bool stopOnFail = false;
        internal static string installer = dirName + @"\dpinst.exe";
        internal static string rollBackDir = @"\RollBacks";
        internal static string rollbackLine = dirName + rollBackDir;
        internal static string stressAppPath = dirName + @"\RandomWriteXML.exe";
        internal static string reStartBAT = dirName + @"\reStart.bat";
        internal static string randomizeList = "false";
        internal static string itCount;
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


        internal static void Main(string[] args)
        {
            ShowWindow(ThisConsole, MAXIMIZE);
            
            try
            {
                Console.WriteLine("this is the dirName now : " + dirName);
                if (!File.Exists(reStartBAT))
                {
                    File.WriteAllText(reStartBAT, "cd " + dirName + Environment.NewLine + stressAppPath);
                }

                string stressLog = "DriverStressLog.txt";
                Logger.AppendToFile = true;
                Logger.LogFileName = stressLog;
                Logger.LogDirName = dirName;

                if (!File.Exists(stressLog))
                {
                    Logger.AddLogFile(stressLog);
                }

                //Logger.LogAll();

                if (!Directory.Exists(dirName)) { Directory.CreateDirectory(dirName); }
                if (!File.Exists(InputTestFilePath))
                {
                    if (args.Length == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("---------------------------------------------------");
                        Console.WriteLine("arguments are required to run this application.");
                        Console.WriteLine("---------------------------------------------------");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("---------------------------------------------------");
                        Console.WriteLine("Arguements must be added in this order only : ");
                        Console.WriteLine("Enter 'true' or 'false' to randomize the list ");
                        Console.WriteLine("Enter the number of times you want this to run");
                        Console.WriteLine("Enter a device\firmware driver name to test first");
                        Console.WriteLine("   e.g. : uefi ");
                        Console.WriteLine(@"this will install\uninstall uefi first each pass");
                        Console.WriteLine("---------------------------------------------------");
                        Console.WriteLine("  press any key to continue...please try again.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        return;
                    }
                    // Command line parsing
                    Arguments CommandLine = new Arguments(args);

                    randomizeList = args[0];
                    randomize = Convert.ToBoolean(randomizeList);

                    itCount = args[1];
                    executionCount = Convert.ToInt32(itCount);

                    startChoice = args[2];

                    supportFolderLOC = dirName;

                    stopOnFailSTR = args[3];
                    stopOnFail = Convert.ToBoolean(stopOnFailSTR);

                    CreateListOrder.RandomizeList(dirName, seedFilePath, randomize, infIndexList, startChoice, executionCount, supportFolderLOC, InputTestFilePath, stopOnFail);
                }
                else
                {
                    XDocument xdoc = XDocument.Load(InputTestFilePath);
                    string infIndexListString = XMLReader.GetSeed(InputTestFilePathBAK);
                    int executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                    string randomizeSTR = xdoc.XPathSelectElement("/Tests/TestChoices/randomizeList").Value.ToString();
                    supportFolderLOC = xdoc.XPathSelectElement("/Tests/TestChoices/SupportFolder").Value.ToString();
                    Array infListFromXML = xdoc.XPathSelectElements("/Tests/InfDirectories/InfDir").ToArray();
                    randomize = Convert.ToBoolean(randomizeSTR);
                    ExecuteFromList.ExecuteTheList(randomize, executionCount, dirName, InputTestFilePath, supportFolderLOC, seedFilePath, startChoice, stopOnFail);
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}
