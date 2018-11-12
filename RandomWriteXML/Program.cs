using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using CommandLine.Utility;

namespace RandomWriteXML
{
    class Program
    {
        internal static string dirName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\InfFiles";
        //internal static string dirName = Environment.CurrentDirectory;
        internal static string InputTestFile = @"\StressTestXML.xml";
        internal static string InputTestFilePath = dirName + InputTestFile;
        internal static string InputTestFilePathBAK = dirName + @"\StressTestXML.xml.BAK";
        internal static string seedFilePath = dirName + @"\SEED.txt";
        internal static string infIndexList = "";
        internal static string stringList = "";
        internal static string startChoice = string.Empty;
        internal static string capStressChoice = string.Empty;
        internal static string supportFolderLOC = string.Empty;
        internal static int executionCount;
        internal static bool randomize = false;
        internal static string installer = dirName + @"\dpinst.exe";
        internal static string rollBackDir = @"\RollBacks";
        internal static string rollbackLine = dirName + rollBackDir;
        internal static string stressAppPath = dirName + @"\RandomWriteXML.exe";
        internal static string[] args = null;

        internal static void Main(string[] args)
        {
            //// Command line parsing
            //Arguments CommandLine = new Arguments(args);

            //// Look for specific arguments values and display them if they exist (return null if they don't)
            //if (CommandLine["RandomizeList"] != null) Console.WriteLine("RandomizeList value: " + CommandLine["RandomizeList"]);
            //else Console.WriteLine("True or False need to be entered to choose random or not random installation order!");

            //if (CommandLine["itCount"] != null) Console.WriteLine("itCount value: " + CommandLine["ExecutionCount"]);
            //else Console.WriteLine("ExecutionCount!");

            //if (CommandLine["startChoice"] != null) Console.WriteLine("startChoice: " + CommandLine["StartChoice"]);
            //else Console.WriteLine("startChoice not defined !");

            //if (CommandLine["size"] != null) Console.WriteLine("Size value: " + CommandLine["size"]);
            //else Console.WriteLine("Size not defined !");

            //if (CommandLine["capStressChoice"] != null) Console.WriteLine("capStressChoice value: " + CommandLine["capStressChoice"]);
            //else Console.WriteLine("capStressChoice !");

            //if (args.Equals((args[0])))
            //{
            //    Console.ForegroundColor = ConsoleColor.Yellow;
            //    Console.WriteLine("---------------------------------------------------");
            //    Console.WriteLine("arguements are required to run this application.");
            //    Console.WriteLine("---------------------------------------------------");
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.WriteLine("---------------------------------------------------");
            //    Console.WriteLine("Arguements must be added in this order only : ");
            //    Console.WriteLine("Enter 'true' or 'false' to randomize the list ");
            //    Console.WriteLine("Enter the number of times you want this to run");
            //    Console.WriteLine("Enter a device\firmware driver name to test first");
            //    Console.WriteLine("   e.g. : uefi ");
            //    Console.WriteLine(@"this will install\uninstall uefi first each pass");
            //    Console.WriteLine("enter the name of a single driver if there is only");
            //    Console.WriteLine("  one being tested as in the case of capStress runs");
            //    Console.WriteLine("---------------------------------------------------");
            //    Console.WriteLine("  press any key to continue...please try again.");
            //    Console.ForegroundColor = ConsoleColor.White;
            //    Console.ReadKey();

            //}

            if (!Directory.Exists(dirName)) { Directory.CreateDirectory(dirName); }
            if (!File.Exists(InputTestFilePath))
            {
                //foreach (string arg in args)
                //{
                    
                    //else
                    //{
                        // Console.WriteLine("Enter randomize choice true or false : ");
                        string randomizeList = args[0];
                        randomize = Convert.ToBoolean(randomizeList);

                        //Console.WriteLine("Enter directory choice : ");
                        supportFolderLOC = dirName;

                        // Console.WriteLine("Enter executionCount choice : ");
                        string itCount = args[1];
                        executionCount = Convert.ToInt32(itCount);

                        // Console.WriteLine("Enter startChoice choice : ");
                        string startChoice = args[2];

                        // Console.WriteLine("Enter capStressChoice choice : ");
                        string capStressChoice = args[3];
                    //}
                //}
                CreateListOrder.RandomizeList(dirName, seedFilePath, randomize, infIndexList, startChoice, capStressChoice, executionCount, supportFolderLOC, InputTestFilePath);
            }
            else
            {
                XDocument xdoc = XDocument.Load(InputTestFilePath);
                string infIndexListString = xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value.ToString();
                string executionCountStr = xdoc.XPathSelectElement("/Tests/TestChoices/ExecutionCount").Value.ToString();
                string randomizeSTR = xdoc.XPathSelectElement("/Tests/TestChoices/randomizeList").Value.ToString();
                supportFolderLOC = xdoc.XPathSelectElement("/Tests/TestChoices/SupportFolder").Value.ToString();
                Array infListFromXML = xdoc.XPathSelectElements("/Tests/InfDirectories/InfDir").ToArray();
                int executionCount = Convert.ToInt32(executionCountStr);
                randomize = Convert.ToBoolean(randomizeSTR);
                ExecuteFromList.ExecuteTheList(randomize, executionCount, dirName, InputTestFilePath, supportFolderLOC, seedFilePath, startChoice);
            }
        }

        //internal static int CMDLineArgs()
        //{
        //    // =====================================================================
        //    // get parameters from user at runtime

        //    Options options = new Options();
        //    CommandLineParser parser = new CommandLineParser(options);
        //    parser.Parse();
        //    Console.WriteLine(parser.UsageInfo.GetHeaderAsString(78));

        //    if (options.Help)
        //    {
        //        Console.WriteLine(parser.UsageInfo.GetOptionsAsString(78));
        //        return 0;
        //    }
        //    else if (parser.HasErrors)
        //    {
        //        Console.WriteLine(parser.UsageInfo.GetErrorsAsString(78));
        //        return -1;
        //    }
        //    Console.WriteLine("Hello {0}!", options.Name);
        //    return 0;

        //    // ---------- try to use args : /help etc..
        //    // =====================================================================
        //}
    }
}
