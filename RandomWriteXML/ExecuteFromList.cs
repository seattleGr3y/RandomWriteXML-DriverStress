using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RandomWriteXML
{
    class ExecuteFromList
    {
        internal static void ExecuteTheList(bool randomize, int executionCount, string desktopPath, string InputTestFilePath, string supportFolderLOC, string seedFilePath)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("am i getting to here... ExecuteTheList...?");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.ReadKey();

            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
            if (executionCount < 1)
            {
                Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dirName + @"RESULTS\DPINST.LOG");
                Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.dirName + @"RESULTS\DriverStressLog.txt");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("at this point...I think I am done...am I?");
                Console.WriteLine("-----------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                File.Create(Program.dirName + "DONE.TXT");
                CheckWhatInstalled.CheckInstalledCSV();
                Console.ReadKey();
                Logger.FunctionLeave();
            }

            while (executionCount > 0)
            {
                executionCount--;
                XDocument xdoc = XDocument.Load(InputTestFilePath);
                string infIndexListString = xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value.ToString();
                xdoc.XPathSelectElement("/Tests/TestChoices/ExecutionCount").Value = executionCount.ToString();
                xdoc.Save(InputTestFilePath);
                if (string.IsNullOrEmpty(infIndexListString))
                {
                    //var infList = xdoc.XPathSelectElements("/Tests/InfDirectories/InfDir");
                    int infListCount = supportFolderLOC.Count();
                    Console.WriteLine("infListCount = " + infListCount);
                    if (randomize == true)
                    {
                        var numbers = new List<int>(Enumerable.Range(1, infListCount));
                        numbers.Shuffle(infListCount);
                        infIndexListString = string.Join(",", numbers.GetRange(0, infListCount));
                    }
                    else
                    {
                        var numbers = new List<int>(Enumerable.Range(1, infListCount));
                        infIndexListString = string.Join(",", numbers.GetRange(0, infListCount));
                    }
                }
                Console.WriteLine("infIndexListString = " + infIndexListString);
                Array list = infIndexListString.Split(',').Select(Int32.Parse).ToArray<int>();
                xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = infIndexListString;
                xdoc.Save(InputTestFilePath);
                foreach (int seedIndex in list)
                {
                    string index = Convert.ToString(seedIndex);
                    int indexLen = index.Length;
                    string stringList = infIndexListString.Remove(0, indexLen).TrimStart(',');
                    Thread.Sleep(100);
                    string indexString = Convert.ToString(index);
                    if (index.Equals(null)) { continue; }

                    string InputTestFilePathBAK = Program.dirName + @"StressTestXML.xml.BAK";
                    List<string> DriverPathList = new List<string>();
                    DriverPathList = XMLReader.GetDriversPath(InputTestFilePath);
                    int driverPathListCount = DriverPathList.Count;
                    int infListCount = DriverPathList.Count;
                    Directory.CreateDirectory(Program.dirName + @"RESULTS");
                    executionCount = XMLReader.GetExecutionCount(InputTestFilePath);

                    File.WriteAllText(seedFilePath, stringList);

                    infIndexListString = File.ReadAllText(seedFilePath);

                    xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = stringList;
                    xdoc.Save(InputTestFilePath);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("indexString : " + indexString);
                    Console.WriteLine("infIndexListString : " + infIndexListString);
                    Console.WriteLine("...this is where we execute testing...");
                    Console.WriteLine("...... well...I think so anyway.......");
                    Console.ForegroundColor = ConsoleColor.White;

                    string line = XMLReader.FromINFIndex(InputTestFilePath, seedIndex);
                    string infName = Path.GetFileName(line);
                    bool isCapsule = GetData.CheckDriverIsFirmware(line);
                    if (isCapsule)
                    {
                        Logger.Comment("re-add the reg key to start post reboot...");
                        string stressAppPath = Program.dirName + @"\DriverStress-2.exe";
                        //RebootAndContinue.SetStartUpRegistry(stressAppPath);
                        Thread.Sleep(3000);
                        CapsuleOrNotInstallCalls.IfIsCapsule(driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);

                    }
                    else
                    {
                        CapsuleOrNotInstallCalls.IsNotCapsule(driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                    }

                }
            }
        }
    }
}
