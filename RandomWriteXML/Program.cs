using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RandomWriteXML
{
    class Program
    {
        internal static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        internal static string dirName = desktopPath + @"\InfFiles\";
        internal static string InputTestFile = @"\StressTestXML.xml";
        internal static string InputTestFilePath = dirName + InputTestFile;
        internal static string seedFilePath = desktopPath + @"\SEED.txt";
        internal static string infIndexList = "";
        internal static string stringList = "";
        internal static string startChoice = string.Empty;
        internal static string capStressChoice = string.Empty;
        internal static string supportFolderLOC = string.Empty;
        internal static bool randomize = false;
        internal static string installer = dirName + @"dpinst.exe";
        internal static string rollBackDir = @"RollBacks";
        internal static string rollbackLine = dirName + rollBackDir;

        internal static void Main(string[] args)
        {
            if (!Directory.Exists(desktopPath)) { Directory.CreateDirectory(desktopPath); }
            if (!File.Exists(dirName + InputTestFilePath))
            {
                Console.WriteLine("Enter randomize choice true or false : ");
                string randomizeList = Console.ReadLine();
                randomize = Convert.ToBoolean(randomizeList);

                Console.WriteLine("Enter directory choice : ");
                supportFolderLOC = Console.ReadLine();

                Console.WriteLine("Enter executionCount choice : ");
                string itCount = Console.ReadLine();
                int executionCount = Convert.ToInt32(itCount);

                Console.WriteLine("Enter startChoice choice : ");
                string startChoice = Console.ReadLine();

                Console.WriteLine("Enter capStressChoice choice : ");
                string capStressChoice = Console.ReadLine();

                int intCount = Convert.ToInt32(executionCount);

                CreateListOrder.RandomizeList(dirName, seedFilePath, randomize, infIndexList, startChoice, capStressChoice, executionCount, supportFolderLOC, InputTestFilePath);
            }
            else
            {
                XDocument xdoc = XDocument.Load(dirName + InputTestFilePath);
                string infIndexListString = xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value.ToString();
                string executionCountStr = xdoc.XPathSelectElement("/Tests/TestChoices/executionCount").Value.ToString();
                string randomizeSTR = xdoc.XPathSelectElement("/Tests/TestChoices/randomizeList").Value.ToString();
                supportFolderLOC = xdoc.XPathSelectElement("/Tests/InfDirectories/SupportFolder").Value.ToString();
                Array infListFromXML = xdoc.XPathSelectElements("/Tests/InfDirectories/InfDir").ToArray();
                int executionCount = Convert.ToInt32(executionCountStr);
                randomize = Convert.ToBoolean(randomizeSTR);
                ExecuteFromList.ExecuteTheList(randomize, executionCount, desktopPath, InputTestFilePath, supportFolderLOC, seedFilePath);
            }
        }
    }
}
