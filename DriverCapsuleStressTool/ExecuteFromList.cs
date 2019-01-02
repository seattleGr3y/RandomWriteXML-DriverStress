using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DriverCapsuleStressTool
{
    class ExecuteFromList
    {
        internal static List<string> capList = new List<string>();

        /// <summary>
        /// executes the stress in the order given by the list that was created
        /// only changed during execution when it finds firmware etc.
        /// </summary>
        /// <param name="randomize"></param>
        /// <param name="executionCount"></param>
        /// <param name="dirName"></param>
        /// <param name="InputTestFilePath"></param>
        /// <param name="supportFolderLOC"></param>
        /// <param name="seedFilePath"></param>
        /// <param name="startChoice"></param>
        internal static void ExecuteTheList(bool randomize, int executionCount, string dirName, string InputTestFilePath, string supportFolderLOC, string seedFilePath, string startChoice)
        {
            if (RegCheck.IsRebootPending())
            {
                Logger.Comment("there is a pending reboot...");
                Thread.Sleep(1000);
                RebootAndContinue.RebootCmd(true);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("... ExecuteTheList...");
            Console.ForegroundColor = ConsoleColor.White;

            if (executionCount == 0)
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
                    //int driverPathListCount;
                    List<string> DriverPathList = new List<string>();
                    string seedStr = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    File.WriteAllText(Program.dirName + @"\" + executionCount + ".txt", seedStr);

                    string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    Thread.Sleep(500);
                    XDocument xdoc = XDocument.Load(Program.InputTestFilePath);
                    int infListCount = XMLReader.GetInfsPathListCount(Program.InputTestFilePathBAK);

                    if (string.IsNullOrEmpty(infIndexListString))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("infListCount = " + infListCount);
                        Console.ForegroundColor = ConsoleColor.White;

                        if (randomize)
                        {
                            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                            //executionCount--;
                            DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                        }
                        else
                        {
                            executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                            //executionCount--;
                            DriverStressInit.RewriteXMLContinue(executionCount, infListCount);
                        }
                    }

                    infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                    Console.WriteLine("infIndexListString = " + infIndexListString);

                    if (randomize)
                    {
                        executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);
                            Thread.Sleep(100);
                            if (index.Equals(null)) { continue; }
                            DriverPathList = GetData.GetInfPathsList(dirName);
                            string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                            string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                            Thread.Sleep(500);
                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileName(line);

                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                            if (isCapsule)
                            {
                                Logger.Comment("this is firmware treat it as such and reboot or rollback\reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);                                
                            }
                            else if (infName.Contains("Surface"))
                            {
                                Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else
                            {
                                Logger.Comment("this is NOT firmware check for reboot afer installed");
                                infListCount--;
                                CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                break;
                            }
                        }
                        Thread.Sleep(500);
                    }


                    else
                    {
                        executionCount = XMLReader.GetExecutionCount(InputTestFilePath);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);
                            Thread.Sleep(100);
                            if (index.Equals(null)) { continue; }
                            Directory.CreateDirectory(Program.desktopPath + @"\RESULTS");

                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount).ToLower();
                            string infName = Path.GetFileNameWithoutExtension(line);
                            string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                            string testInfName = infName.ToLower();
                            infIndexListString = File.ReadAllText(seedFilePath);

                            if (line.Contains(testIsStartChoice.ToLower()))
                            {
                                Logger.Comment("This is the start first choice : " + line);
                                bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);

                                if (isCapsule)
                                {
                                    infListCount--;
                                    Logger.Comment("re-add the reg key to start post reboot...");
                                    Thread.Sleep(500);
                                    capList.Add(line);
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                }
                                else if (infName.Contains("Surface"))
                                {
                                    Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                    Thread.Sleep(500);
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("NOT MATCHING to the startChoice");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    infListCount--;
                                    CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(1000);

                        infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                        foreach (int seedIndex in infIndexListString.Split(',').Select(Int32.Parse).ToList<int>())
                        {
                            string index = Convert.ToString(seedIndex);

                            Thread.Sleep(100);
                            string indexString = Convert.ToString(index);
                            if (index.Equals(null)) { continue; }
                            
                            string line = XMLReader.FromINFIndex(infListCount, InputTestFilePath, seedIndex, executionCount).ToLower();
                            bool isCapsule = GetData.CheckDriverIsFirmware(line, executionCount, infListCount);
                            string infName = Path.GetFileName(line);
                            string testIsStartChoice = GetData.GetTestFirst(InputTestFilePath);
                            string testInfName = Path.GetFileNameWithoutExtension(testIsStartChoice).ToLower();
                            Thread.Sleep(500);

                            if (isCapsule)
                            {
                                Logger.Comment("this is firmware and will need to reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);                                
                            }
                            else if (infName.Contains("Surface"))
                            {
                                Logger.Comment("'Surface' in the name of this driver so to be safe reboot or rollback\reboot...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IfIsCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                            }
                            else
                            {
                                Logger.Comment("THIS IS NOT FIRMWARE...");
                                Thread.Sleep(500);
                                infListCount--;
                                CapsuleOrNotInstallCalls.IsNotCapsule(seedIndex, infIndexListString, infListCount, infName, DriverPathList, line, Program.InputTestFilePathBAK, Program.installer, executionCount, Program.dirName, Program.startChoice, Program.rollbackLine, InputTestFilePath);
                                break;
                            }
                        }
                        Thread.Sleep(500);
                    }
                    executionCount--;
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}
