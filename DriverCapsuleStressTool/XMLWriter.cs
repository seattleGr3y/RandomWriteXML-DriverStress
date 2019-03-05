using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DriverCapsuleStressTool
{
    class XMLWriter
    {
        /// <summary>
        /// creates the XML that this executable uses to actually run the testing
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="randomize"></param>
        /// <param name="seedFileText"></param>
        /// <param name="stringList"></param>
        /// <param name="startChoice"></param>
        /// <param name="executionCount"></param>
        /// <param name="supportFolderLOC"></param>
        /// <param name="InputTestFilePath"></param>
        internal static void CreateXML(string dirName, bool randomize, string seedFileText, string stringList, string startChoice, int executionCount, string supportFolderLOC, string InputTestFilePath, string stopOnErrorSTR, string groupFirmwareSTR, string dumpFilePath)
        {
            string lastInstalled = string.Empty;
            string dumpExist = "False";
            string logString = string.Empty;
            string RegistryErrorCode = string.Empty;
            string infName = string.Empty;
            XmlWriter xmlWriter = XmlWriter.Create(InputTestFilePath);
            int infIndex = 0;

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteWhitespace("\n");
            xmlWriter.WriteStartElement("Tests");
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("InfDirectories");
            xmlWriter.WriteWhitespace("\n");

            List<string> infsPathList = GetData.GetInfPathsList(dirName);
            int infsPathListCount = infsPathList.Count;

            foreach (string infDir in infsPathList)
            {
                infName = Path.GetFileNameWithoutExtension(infDir);
                //string friendlyInfName = GetData.FindFriendlyNameInCSV(infName);
                // TRIED TO USE THE DICTIONARY BUT I CAN HAVE DUP VALUES FOR INF NAMES
                // IN CSV FILE WITHOUT ISSUE BUT DICTIONARY FAILS WITH DUP VALUES
                string friendlyInfName = Names_Dictionary.GetNamesMatch(infName);
                infIndex++;
                xmlWriter.WriteStartElement("InfDir");
                xmlWriter.WriteAttributeString("InfName", friendlyInfName);
                xmlWriter.WriteAttributeString("InfPath", infDir);
                xmlWriter.WriteAttributeString("InfIndex", infIndex.ToString());
                xmlWriter.WriteAttributeString("ErrorCount", "0");
                xmlWriter.WriteAttributeString("FailedCount", "0");
                xmlWriter.WriteAttributeString("SuccessfullInstalls", "0");
                xmlWriter.WriteAttributeString("SuccessfullUninstalls", "0");
                xmlWriter.WriteAttributeString("RollbacksErrorCount", "0");
                xmlWriter.WriteAttributeString("RollbacksFailedCount", "0");
                xmlWriter.WriteAttributeString("SuccessfullRollbacks", "0");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteWhitespace("\n");
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("TestChoices");
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("SupportFolder");
            xmlWriter.WriteString(supportFolderLOC);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("randomizeList");
            xmlWriter.WriteString(randomize.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("GroupFirmware");
            xmlWriter.WriteString(groupFirmwareSTR);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("StartChoice");
            xmlWriter.WriteString(startChoice);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("StopOnError");
            xmlWriter.WriteString(stopOnErrorSTR);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("ExecutionCount");
            xmlWriter.WriteString(executionCount.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("StartSeed");
            xmlWriter.WriteString(seedFileText);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("CurrentSeed");
            xmlWriter.WriteString(stringList);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("DumpExists");
            xmlWriter.WriteString(dumpExist);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("DumpFilePath");
            xmlWriter.WriteString(dumpFilePath);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("FailedErroredINFs");
            xmlWriter.WriteString(logString);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("InfsPathListCount");
            xmlWriter.WriteString(infsPathListCount.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("LastInstalled");
            xmlWriter.WriteString(lastInstalled);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("RegistryErrorCodes");
            xmlWriter.WriteAttributeString("RegistryErrorCode", RegistryErrorCode);
            xmlWriter.WriteAttributeString("FailedFirmware", infName);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("check the XML...");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// if no choice is set and there is firmware set highest pri firmware to install first
        /// for now this will default to UEFI
        /// XMLWriter.SetTestFirst(InputTestFilePathBAK, InputTestFilePath, infName);
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <param name="infName"></param>
        /// <returns></returns>
        internal static void SetTestFirst(string infName)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(Program.InputTestFilePathBAK);
                Logger.Comment("setting this to test first as there was no choice made : " + infName);
                testInputData.XPathSelectElement("/Tests/TestChoices/StartChoice").Value = infName;
                testInputData.Save(Program.InputTestFilePathBAK);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// if no choice is set and there is firmware set highest pri firmware to install first
        /// for now this will default to UEFI
        /// XMLWriter.SetRegErrCode(RegistryErrorCode, line);
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <param name="infName"></param>
        /// <returns></returns>
        internal static void SetRegErrCode(string RegistryErrorCode, string line)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(Program.InputTestFilePathBAK);
                var regCodes = testInputData.Descendants("RegistryErrorCodes");
                foreach (var regCode in regCodes.Elements())
                {
                    regCode.Attribute("RegistryErrorCode").Value = RegistryErrorCode;
                    regCode.Attribute("FailedFirmware").Value = line;
                }

                testInputData.Save(Program.InputTestFilePathBAK);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// if no choice is set and there is firmware set highest pri firmware to install first
        /// for now this will default to UEFI
        /// XMLWriter.SetLastInstalled(line);
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <param name="infName"></param>
        /// <returns></returns>
        internal static void SetLastInstalled(string line)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(Program.InputTestFilePathBAK);
                Logger.Comment("setting this to test first as there was no choice made : " + line);
                testInputData.XPathSelectElement("/Tests/TestChoices/LastInstalled").Value = line;
                testInputData.Save(Program.InputTestFilePathBAK);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePathBAK"></param>
        /// XMLWriter.LogResults(InputTestFilePathBAK, errorCount, failedCount, passedCount, dumpExist);
        /// <returns></returns>
        internal static void LogResults(string InputTestFilePathBAK, string errorCount, string failedCount, string successInstallResults, string successUninstallResults,
            string rollBackErrorCount, string rollBackFailedCount, string rollBackSuccessCount, string dumpExist, string logString, string dumpFilePath)
        {
            try 
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);

                var driversPathList = testInputData.Descendants("InfDirectories");
                foreach (var driverPath in driversPathList.Elements())
                {
                    driverPath.Attribute("ErrorCount").Value = errorCount;
                    driverPath.Attribute("FailedCount").Value = failedCount;
                    driverPath.Attribute("SuccessfullInstalls").Value = successInstallResults;
                    driverPath.Attribute("SuccessfullUninstalls").Value = successUninstallResults;
                    driverPath.Attribute("RollbacksErrorCount").Value = rollBackErrorCount;
                    driverPath.Attribute("RollbacksFailedCount").Value = rollBackFailedCount;
                    driverPath.Attribute("SuccessfullRollbacks").Value = rollBackSuccessCount;
                    testInputData.Save(InputTestFilePathBAK);
                }
                testInputData.XPathSelectElement("/Tests/TestChoices/DumpExists").Value = dumpExist;
                testInputData.XPathSelectElement("/Tests/TestChoices/DumpFilePath").Value = dumpFilePath;
                testInputData.XPathSelectElement("/Tests/TestChoices/FailedErroredINFs").Value = logString;
                testInputData.Save(InputTestFilePathBAK);
                Thread.Sleep(500);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePathBAK"></param>
        /// <returns></returns>
        internal static void SaveSeed(string InputTestFilePathBAK, string StartSeed, string currentSeed)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                testInputData.XPathSelectElement("/Tests/TestChoices/StartSeed").Value = StartSeed;
                testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = currentSeed;
                testInputData.Save(InputTestFilePathBAK);
                Logger.Comment("randomized list to save for re-use if need be : " + StartSeed);
                Logger.FunctionLeave();
                //return seed;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                //return null;
            }
        }

        /// <summary>
        /// decrease the testRunLoop count so that each time we loop through the list
        /// it will know to execute the list the correct number of remaining times
        /// </summary>
        /// <param name="InputTestFilePathBAK"></param>
        /// <param name="testCount"></param>
        /// <returns></returns>
        internal static int DecrementExecutionCount(string InputTestFilePathBAK, int executionCount)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                int testRunLoop = int.Parse(testInputData.XPathSelectElement("/Tests/TestChoices/ExecutionCount").Value);
                executionCount--;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("executionCount :: " + executionCount);
                Console.WriteLine("within the decrementExecutionCount method");
                Console.WriteLine("should be getting smaller correctly");
                Console.ForegroundColor = ConsoleColor.White;
                //Thread.Sleep(10000);
                string executionCountSTR = Convert.ToString(executionCount);
                testInputData.XPathSelectElement("/Tests/TestChoices/ExecutionCount").Value = executionCountSTR;
                Logger.Comment("=======================================");
                Logger.Comment("this is what the loop count is now : " + executionCountSTR);
                Logger.Comment("=======================================");
                testInputData.Save(InputTestFilePathBAK);
                Logger.FunctionLeave();
                return executionCount;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                return 0;
            }
        }

        /// <summary>
        /// remove the element from the XML containing the driver that was just installed
        /// or uninstalled so when the app loops through the list again it does not try
        /// to reinstall or uninstall out of turn or get stuck in a loop
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <param name="infName"></param>
        internal static void RemoveXMLElemnt(string InputTestFilePath, string line, int seedIndex)
        {
            try
            {
                string tmpInfName = Path.GetFileNameWithoutExtension(line).ToLower();
                var testInputData = XDocument.Load(InputTestFilePath);
                var driversPathList = testInputData.Descendants("InfDirectories");

                foreach (var driverPath in driversPathList.Elements())
                {
                    string fullInfPath = driverPath.Attribute("InfPath").Value;
                    string DriverInfPath = Path.GetFileNameWithoutExtension(fullInfPath).ToLower();
                    if (DriverInfPath.Contains(tmpInfName))
                    {
                        driverPath.Remove();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("######################################");
                        Console.WriteLine("Remove XML Element for : " + tmpInfName);
                        Console.WriteLine("######################################");
                        Console.ForegroundColor = ConsoleColor.White;
                        //Thread.Sleep(2000);
                        testInputData.Save(InputTestFilePath);
                        //Thread.Sleep(500);
                        UpdateSeedXML(seedIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// update the 'seed' in the XML to track execution progress and keep running in the correct order
        /// </summary>
        /// <param name="seedIndex"></param>
        internal static void UpdateSeedXML(int seedIndex)
        {
            try
            {
                string currentSeed = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                string seedIndexSTR = Convert.ToString(seedIndex);
                int seedLocIndex = currentSeed.IndexOf(seedIndexSTR);
                currentSeed = currentSeed.Replace(seedIndexSTR, "");
                currentSeed = currentSeed.Replace(",,", ",");
                currentSeed = currentSeed.TrimEnd(',').TrimStart(',');

                string StartSeed = XMLReader.GetStartSeed(Program.InputTestFilePathBAK);

                var testInputData = XDocument.Load(Program.InputTestFilePathBAK);
                testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = currentSeed;
                //Thread.Sleep(500);
                testInputData.Save(Program.InputTestFilePathBAK);

                //File.WriteAllText(Program.seedFilePath, currentSeed);
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}