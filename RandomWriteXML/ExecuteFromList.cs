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
        internal static List<string> capList = new List<string>();

        internal static void ExecuteTheList(bool randomize, int executionCount, string dirName, string InputTestFilePath, string supportFolderLOC, string seedFilePath, string startChoice)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("am i getting to here... ExecuteTheList...?");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.ReadKey();

            List<string> DriverPathList = new List<string>();
            DriverPathList = XMLReader.GetDriversPath(InputTestFilePath);
            int driverPathListCount = DriverPathList.Count;
            
            if (executionCount < 1)
            {
                Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dirName + @"\RESULTS\DPINST.LOG");
                Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.dirName + @"\RESULTS\DriverStressLog.txt");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("at this point...I think I am done...am I?");
                Console.WriteLine("-----------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                File.Create(Program.dirName + @"\DONE.TXT");
                CheckWhatInstalled.CheckInstalledCSV();
                Console.ReadKey();
                Logger.FunctionLeave();
            }

            while (executionCount > 0)
            {
                executionCount--;
                string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                //string infIndexListString = xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value.ToString();

                XMLWriter.DecrementExecutionCount(Program.InputTestFilePathBAK, executionCount);

                if (string.IsNullOrEmpty(infIndexListString))
                {
                    XDocument xdoc = XDocument.Load(InputTestFilePath);
                    var infList = xdoc.XPathSelectElements("/Tests/InfDirectories/InfDir");
                    int infListCount = infList.Count();
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
                //XMLWriter.SaveCurrentSeed(InputTestFilePath, Program.InputTestFilePathBAK, infIndexListString);
                //xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = infIndexListString;
                //xdoc.Save(InputTestFilePath);


                foreach (int seedIndex in list)
                {
                    string index = Convert.ToString(seedIndex);
                    int indexLen = index.Length;
                    string stringList = infIndexListString.Remove(0, indexLen).TrimStart(',');
                    Thread.Sleep(100);
                    string indexString = Convert.ToString(index);
                    if (index.Equals(null)) { continue; }
                    string InputTestFilePathBAK = Program.dirName + @"\StressTestXML.xml.BAK";

                    XMLWriter.SaveCurrentSeed(InputTestFilePath, InputTestFilePathBAK, stringList);

                    DriverPathList = new List<string>();
                    DriverPathList = XMLReader.GetDriversPath(InputTestFilePath);
                    driverPathListCount = DriverPathList.Count;
                    int infListCount = DriverPathList.Count;
                    string testIsStartChoice = XMLReader.FromINFIndex(InputTestFilePath, seedIndex);
                    string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("testInfName : " + testInfName);
                    Console.ForegroundColor = ConsoleColor.White;
                    Thread.Sleep(2000);
                    //string line = XMLReader.FromINFIndex(InputTestFilePath, seedIndex);
                    //string infName = Path.GetFileName(line);
                    //bool isCapsule = GetData.CheckDriverIsFirmware(line);

                    if (randomize == true)
                    {
                        index = Convert.ToString(seedIndex);
                        indexLen = index.Length;
                        string currentSeed = infIndexListString.Remove(0, indexLen).TrimStart(',');
                        Thread.Sleep(100);
                        indexString = Convert.ToString(index);
                        if (index.Equals(null)) { continue; }

                        InputTestFilePathBAK = Program.dirName + @"\StressTestXML.xml.BAK";
                        DriverPathList = new List<string>();
                        DriverPathList = XMLReader.GetDriversPath(InputTestFilePath);
                        driverPathListCount = DriverPathList.Count;
                        infListCount = DriverPathList.Count;
                        Directory.CreateDirectory(Program.dirName + @"\RESULTS");
                        executionCount = XMLReader.GetExecutionCount(InputTestFilePath);

                        File.WriteAllText(seedFilePath, stringList);

                        infIndexListString = File.ReadAllText(seedFilePath);

                        //XDocument xdoc = XDocument.Load(InputTestFilePath);
                        //xdoc.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = stringList;
                        //xdoc.Save(InputTestFilePath);
                        XMLWriter.SaveCurrentSeed(InputTestFilePath, InputTestFilePathBAK, currentSeed);

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
                            XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                            Logger.Comment("re-add the reg key to start post reboot...");
                            Thread.Sleep(3000);
                            CapsuleOrNotInstallCalls.IfIsCapsule(driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);

                        }
                        else
                        {
                            XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                            CapsuleOrNotInstallCalls.IsNotCapsule(driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                        }
                    }

                    else if (randomize == false)
                    {
                        if (testInfName.Contains(startChoice.ToLower()))
                        {
                            string line = XMLReader.FromINFIndex(InputTestFilePath, seedIndex);
                            string infName = Path.GetFileName(line);
                            bool isCapsule = GetData.CheckDriverIsFirmware(line);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("i matched the INF to the startChoice");
                            Console.ForegroundColor = ConsoleColor.White;
                            XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);

                            if (isCapsule)
                            {
                                Logger.Comment("re-add the reg key to start post reboot...");
                                Thread.Sleep(1000);
                                CapsuleOrNotInstallCalls.IfIsCapsule(driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                capList.Add(line);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("NOT MATCHING to the startChoice");
                                Console.ForegroundColor = ConsoleColor.White;
                                continue;
                            }
                        }
                        else { continue; }
                    }
                }

                if (randomize == false)
                {
                    foreach (int seedIndex in list)
                    {
                        string index = Convert.ToString(seedIndex);
                        int indexLen = index.Length;
                        string stringList = infIndexListString.Remove(0, indexLen).TrimStart(',');
                        Thread.Sleep(100);
                        string indexString = Convert.ToString(index);
                        if (index.Equals(null)) { continue; }

                        string InputTestFilePathBAK = Program.dirName + @"\StressTestXML.xml.BAK";
                        DriverPathList = new List<string>();
                        DriverPathList = XMLReader.GetDriversPath(InputTestFilePath);
                        driverPathListCount = DriverPathList.Count;
                        int infListCount = DriverPathList.Count;
                        Directory.CreateDirectory(Program.dirName + @"\RESULTS");
                        executionCount = XMLReader.GetExecutionCount(InputTestFilePath);

                        File.WriteAllText(seedFilePath, stringList);

                        infIndexListString = File.ReadAllText(seedFilePath);

                        XMLWriter.SaveCurrentSeed(InputTestFilePath, InputTestFilePathBAK, stringList);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("indexString : " + indexString);
                        Console.WriteLine("infIndexListString : " + stringList);
                        Console.ForegroundColor = ConsoleColor.White;

                        string line = XMLReader.FromINFIndex(InputTestFilePath, seedIndex);
                        string infName = Path.GetFileName(line);
                        XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                        bool isCapsule = GetData.CheckDriverIsFirmware(line);
                        if (isCapsule)
                        {
                            Logger.Comment("re-add the reg key to start post reboot...");
                            Thread.Sleep(3000);
                            CapsuleOrNotInstallCalls.IfIsCapsule(driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                        }
                        else
                        {
                            //continue;
                            CapsuleOrNotInstallCalls.IsNotCapsule(driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                        }
                    }
                }
            }
        }
    }
}
