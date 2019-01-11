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
        internal static void CreateXML(string dirName, bool randomize, string seedFileText, string stringList, string startChoice, int executionCount, string supportFolderLOC, string InputTestFilePath)
        {
            string failedCount = "0";
            string successInstallResults = "0";
            string successUninstallResults = "0";
            string errorCount = "0";
            bool dumpsExist = false;
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
                string infName = Path.GetFileNameWithoutExtension(infDir);
                string friendlyInfName = GetData.FindFriendlyNameInCSV(infName);
                infIndex++;
                xmlWriter.WriteStartElement("InfDir");
                xmlWriter.WriteAttributeString("InfName", friendlyInfName);
                xmlWriter.WriteAttributeString("InfPath", infDir);
                xmlWriter.WriteAttributeString("InfIndex", infIndex.ToString());
                xmlWriter.WriteAttributeString("InstallCount", "0");
                xmlWriter.WriteAttributeString("UnInstallCount", "0");
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
            xmlWriter.WriteString(Program.groupFirmware.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("StartChoice");
            xmlWriter.WriteString(startChoice);
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

            xmlWriter.WriteStartElement("ErrorCount");
            xmlWriter.WriteString(errorCount);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("SuccessfullInstall");
            xmlWriter.WriteString(successInstallResults);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("SuccessfullUninstall");
            xmlWriter.WriteString(successUninstallResults);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("FailedCount");
            xmlWriter.WriteString(failedCount);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("DumpsExists");
            xmlWriter.WriteString(dumpsExist.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("InfsPathListCount");
            xmlWriter.WriteString(infsPathListCount.ToString());
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

        #region TESTING THIS METHOD LATER
        /// <summary>
        /// if no choice is set and there is firmware set highest pri firmware to install first
        /// for now this will default to UEFI
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <param name="infName"></param>
        /// <returns></returns>
        internal static void SetTestFirst(string InputTestFilePathBAK, string InputTestFilePath, string infName)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePath);
                Logger.Comment("setting this to test first as there was no choice made : " + infName);
                testInputData.XPathSelectElement("/DriverTests/Test/TestFirst").Value = infName;
                testInputData.Save(InputTestFilePath);
                Utilities.CopyFile(InputTestFilePath, InputTestFilePathBAK);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
        #endregion

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePathBAK"></param>
        /// XMLWriter.LogResults(InputTestFilePathBAK, errorCount, failedCount, passedCount, dumpExist);
        /// <returns></returns>
        internal static void LogResults(string InputTestFilePathBAK, string errorCount, string failedCount, string successUninstallResults, string successInstallResults, string dumpExist)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                testInputData.XPathSelectElement("/Tests/TestChoices/ErrorCount").Value = errorCount;
                testInputData.XPathSelectElement("/Tests/TestChoices/FailedCount").Value = failedCount;
                testInputData.XPathSelectElement("/Tests/TestChoices/SuccessfullInstalls").Value = successInstallResults;
                testInputData.XPathSelectElement("/Tests/TestChoices/SuccessfullUninstalls").Value = successUninstallResults;
                testInputData.XPathSelectElement("/Tests/TestChoices/DumpExists").Value = dumpExist;
                testInputData.Save(InputTestFilePathBAK);

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
                        Thread.Sleep(500);
                        testInputData.Save(InputTestFilePath);
                        Thread.Sleep(500);
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
            string currentSeed = XMLReader.GetSeed(Program.InputTestFilePathBAK);
            string seedIndexSTR = Convert.ToString(seedIndex);
            int seedLocIndex = currentSeed.IndexOf(seedIndexSTR);
            currentSeed = currentSeed.Replace(seedIndexSTR, "");
            currentSeed = currentSeed.Replace(",,", ",");
            currentSeed = currentSeed.TrimEnd(',').TrimStart(',');

            string StartSeed = XMLReader.GetStartSeed(Program.InputTestFilePathBAK);

            var testInputData = XDocument.Load(Program.InputTestFilePathBAK);
            testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = currentSeed;
            Thread.Sleep(500);
            testInputData.Save(Program.InputTestFilePathBAK);

            File.WriteAllText(Program.seedFilePath, currentSeed);
        }

        /// <summary>
        /// Tracking number of times each inf is installed 
        /// </summary>
        /// <param name="InputTestFilePathBAK"></param>
        /// <param name="infName"></param>
        internal static void SetInstallCount(string InputTestFilePathBAK, int seedIndex)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                var driversPathList = testInputData.Descendants("InfDirectories");
                string seedIndexStr = Convert.ToString(seedIndex);
                int currentInstallCount = 0;

                foreach (var driverPath in driversPathList.Elements())
                {
                    string indexFromXML = driverPath.Attribute("InfIndex").Value;

                    if (indexFromXML.Equals(seedIndexStr))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("seedIndexStr SetInstallCount method : " + seedIndexStr);
                        Console.WriteLine("indexFromXML SetInstallCount method : " + indexFromXML);
                        Console.ForegroundColor = ConsoleColor.White;

                        string currentInstallCountSTR = driverPath.Attribute("InstallCount").Value;
                        currentInstallCount = Convert.ToInt32(currentInstallCountSTR);
                        currentInstallCount++;
                        driverPath.Attribute("InstallCount").Value = currentInstallCount.ToString();
                        testInputData.Save(InputTestFilePathBAK);
                        Thread.Sleep(2000);
                    }
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// Tracking number of times each inf is uninstalled 
        /// </summary> 
        /// <param name="InputTestFilePathBAK"></param>
        /// <param name="infName"></param>
        internal static void SetUnInstallCount(string InputTestFilePathBAK, int seedIndex)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                var driversPathList = testInputData.Descendants("InfDirectories");
                string seedIndexStr = Convert.ToString(seedIndex);
                int currentUnInstallCount = 0;

                foreach (var driverPath in driversPathList.Elements())
                {
                    string indexFromXML = driverPath.Attribute("InfIndex").Value;

                    if (indexFromXML.Equals(seedIndexStr))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("seedIndexStr SetInstallCount method : " + seedIndexStr);
                        Console.WriteLine("indexFromXML SetInstallCount method : " + indexFromXML);
                        Console.ForegroundColor = ConsoleColor.White;

                        string currentUnInstallCountSTR = driverPath.Attribute("UnInstallCount").Value;
                        currentUnInstallCount = Convert.ToInt32(currentUnInstallCountSTR);
                        currentUnInstallCount++;
                        driverPath.Attribute("UnInstallCount").Value = currentUnInstallCount.ToString();
                        testInputData.Save(InputTestFilePathBAK);
                        Thread.Sleep(2000);
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