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
            Console.WriteLine("... ExecuteTheList...");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.ReadKey();

            if (executionCount < 1)
            {
                Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
                Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.desktopPath + @"\RESULTS\DPINST.LOG");
                Utilities.CopyFile(Program.dirName + @"\DriverStressLog.txt", Program.desktopPath + @"\RESULTS\DriverStressLog.txt");
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

            try
            {
                while (executionCount > 0)
                {

                    //List<string> DriverPathList = new List<string>();
                    //DriverPathList = GetData.GetInfPathsList(dirName);
                    int driverPathListCount;
                    List<string> DriverPathList = new List<string>();

                    string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    XDocument xdoc = XDocument.Load(Program.InputTestFilePathBAK);
                    var infList = xdoc.XPathSelectElements("/Tests/InfDirectories/InfDir");
                    int infListCount = infList.Count();

                    if (string.IsNullOrEmpty(infIndexListString))
                    {
                        Console.WriteLine("infListCount = " + infListCount);

                        if (infListCount == 0)
                        {
                            DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                        }

                        if (randomize)
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

                    foreach (int seedIndex in list)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("#######################################################");
                        Console.WriteLine(" INSIDE THE FIRST FOREACH seedIndex in list LOOP NOW : ");
                        Console.WriteLine("#######################################################");
                        Console.ForegroundColor = ConsoleColor.White;

                        string index = Convert.ToString(seedIndex);

                        Thread.Sleep(100);
                        string indexString = Convert.ToString(index);
                        if (index.Equals(null)) { continue; }


                        driverPathListCount = 0;
                        DriverPathList = GetData.GetInfPathsList(dirName);
                        driverPathListCount = DriverPathList.Count;

                        XMLReader.GetDriversPath(InputTestFilePath, executionCount, infListCount);
                        string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount, driverPathListCount);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("inf index # : " + index);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("line : " + line);
                        Console.ForegroundColor = ConsoleColor.White;

                        string infName = Path.GetFileName(line);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("driversPathListCount : " + driverPathListCount);
                        Console.ForegroundColor = ConsoleColor.White;
                        string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                        string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                        //if (string.IsNullOrEmpty(testInfName)) { break; }
                        Thread.Sleep(500);

                        if (randomize == true)
                        {
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");
                            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);

                            if (isCapsule)
                            {
                                Logger.Comment("re-add the reg key to start post reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, driverPathListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else
                            {
                                infListCount--;
                                CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, driverPathListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                        }

                        else if (randomize == false)
                        {
                            if (infName.Contains(testIsStartChoice.ToLower()))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("i matched the INF to the startChoice");
                                Console.WriteLine("inf index # : " + index);
                                Console.WriteLine("testfirst choice : " + testInfName);
                                Console.ForegroundColor = ConsoleColor.White;

                                //string trimCurrentSeed = infIndexListString.Replace(seedIndex.ToString() + ',', "");
                                //string currentSeed = trimCurrentSeed.TrimEnd(',').TrimStart(',');
                                //list = currentSeed.Split(',').Select(Int32.Parse).ToArray<int>();
                                bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                                if (isCapsule)
                                {
                                    infListCount--;
                                    Logger.Comment("re-add the reg key to start post reboot...");
                                    Thread.Sleep(500);
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, driverPathListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                    capList.Add(line);
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("NOT MATCHING to the startChoice");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, driverPathListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("this is the infListCount: " + infListCount);
                        Console.ForegroundColor = ConsoleColor.White;
                        Thread.Sleep(500);

                        if (infListCount == 0)
                        {
                            DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                        }
                    }

                    #region CHANGED SOME OF THE LOGIC - THIS IS UNNECCESSARY I HOPE
                    if (randomize == false)
                    {
                        foreach (int seedIndex in list)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("#######################################################");
                            Console.WriteLine(" INSIDE THE SECOND FOREACH seedIndex in list LOOP NOW : ");
                            Console.WriteLine("#######################################################");
                            Console.ForegroundColor = ConsoleColor.White;

                            driverPathListCount = 0;
                            DriverPathList = GetData.GetInfPathsList(dirName);
                            driverPathListCount = DriverPathList.Count;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("this is the infListCount now : " + driverPathListCount);
                            Console.ForegroundColor = ConsoleColor.White;

                            string index = Convert.ToString(seedIndex);
                            Thread.Sleep(100);
                            string indexString = Convert.ToString(index);
                            if (index.Equals(null)) { continue; }
                            string InputTestFilePathBAK = Program.dirName + @"\StressTestXML.xml.BAK";
                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");

                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount, driverPathListCount);

                            XMLReader.GetDriversPath(InputTestFilePath, executionCount, infListCount);

                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            string infName = Path.GetFileName(line);
                            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                            infIndexListString = File.ReadAllText(seedFilePath);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("inf index # : " + index);
                            Console.WriteLine("infName : " + infName);
                            Console.ForegroundColor = ConsoleColor.White;

                            string trimCurrentSeed = infIndexListString.Replace(seedIndex.ToString() + ',', "");
                            string currentSeed = trimCurrentSeed.TrimEnd(',').TrimStart(',');
                            list = currentSeed.Split(',').Select(Int32.Parse).ToArray<int>();

                            if (isCapsule)
                            {
                                //XMLWriter.RemoveXMLElemnt(InputTestFilePath, infName);
                                Logger.Comment("this is firmware and will need to reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, driverPathListCount, infName, DriverPathList, line, InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else
                            {
                                continue;
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("this is the infListCount: " + infListCount);
                            Console.ForegroundColor = ConsoleColor.White;
                            Thread.Sleep(500);

                            if (infListCount == 0)
                            {
                                DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                            }
                        }

                        foreach (int seedIndex in list)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("#######################################################");
                            Console.WriteLine(" INSIDE THE THIRD FOREACH seedIndex in list LOOP NOW : ");
                            Console.WriteLine("#######################################################");
                            Console.ForegroundColor = ConsoleColor.White;

                            //string trimCurrentSeed = infIndexListString.Replace(seedIndex.ToString() + ',', "");
                            //string currentSeed = trimCurrentSeed.TrimEnd(',').TrimStart(',');
                            //list = currentSeed.Split(',').Select(Int32.Parse).ToArray<int>();

                            string index = Convert.ToString(seedIndex);

                            Thread.Sleep(100);
                            string indexString = Convert.ToString(index);
                            if (index.Equals(null)) { continue; }

                            driverPathListCount = 0;
                            DriverPathList = new List<string>();
                            DriverPathList = GetData.GetInfPathsList(dirName);
                            driverPathListCount = DriverPathList.Count;

                            XMLReader.GetDriversPath(InputTestFilePath, executionCount, infListCount);

                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount, driverPathListCount);
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            string infName = Path.GetFileName(line);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("driversPathListCount : " + driverPathListCount);
                            Console.ForegroundColor = ConsoleColor.White;
                            string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                            string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                            //if (string.IsNullOrEmpty(testInfName)) { break; }
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("inf index # : " + index);
                            Console.WriteLine("test is this the first choice : " + testInfName);
                            Thread.Sleep(500);
                            Logger.Comment("THIS IS NOT FIRMWARE...");

                            Thread.Sleep(500);
                            infListCount--;
                            CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, driverPathListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("this is the infListCount: " + infListCount);
                        Console.ForegroundColor = ConsoleColor.White;
                        Thread.Sleep(500);

                        if (infListCount == 0)
                        {
                            DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}
