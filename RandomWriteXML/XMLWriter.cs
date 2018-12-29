﻿using Microsoft.HWSW.Test.Utilities;
using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RandomWriteXML
{
    class XMLWriter
    {
        internal static void CreateXML(string dirName, bool randomize, string seedFileText, string stringList, string startChoice, int executionCount, string supportFolderLOC, string InputTestFilePath)
        {
            XmlWriter xmlWriter = XmlWriter.Create(InputTestFilePath);
            int infIndex = 0;

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteWhitespace("\n");
            xmlWriter.WriteStartElement("Tests");
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteStartElement("InfDirectories");
            xmlWriter.WriteWhitespace("\n");
            
            foreach (string infDir in GetData.GetInfPathsList(dirName))
            {
                string infName = Path.GetFileName(infDir);
                infIndex++;
                xmlWriter.WriteStartElement("InfDir");
                xmlWriter.WriteAttributeString("InfName", infName);
                xmlWriter.WriteAttributeString("InfPath", infDir);
                xmlWriter.WriteAttributeString("InfIndex", infIndex.ToString());
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

            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\n");

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("check the XML...");
            Console.ForegroundColor = ConsoleColor.White;
        }

        #region TESTING THIS METHOD 10/17/2018
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

        internal static void SaveCurrentSeed(string StartSeed, string InputTestFilePathBAK, string currentSeed)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = currentSeed;
                string StartSeedSTR = testInputData.XPathSelectElement("/Tests/TestChoices/StartSeed").Value.ToString();
                Thread.Sleep(500);
                testInputData.Save(InputTestFilePathBAK);
                Thread.Sleep(500);
                Logger.Comment("Starting list was this  : " + StartSeedSTR);
                Logger.Comment("Current list to execute : " + currentSeed);
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
                Logger.Comment("changing testRunLoop : " + executionCountSTR + " to testRunLoop - 1 ");
                Logger.Comment("=====================================");
                Logger.Comment("this is what the loop count is now : " + executionCountSTR);
                Logger.Comment("CHECK THE XML... ");
                Logger.Comment("=====================================");
                testInputData.Save(InputTestFilePathBAK);
                //Console.ReadKey();
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
        internal static void RemoveXMLElemnt(string InputTestFilePath, string infName)
        {
            try 
            {
                var testInputData = XDocument.Load(InputTestFilePath);
                var driversPathList = testInputData.Descendants("InfDirectories");

                foreach (var driverPath in driversPathList.Elements())
                {
                    string DriverInfPath = driverPath.Attribute("InfPath").Value;
                    if (DriverInfPath.Contains(infName))
                    {
                        driverPath.Remove();
                    }
                }
                Thread.Sleep(500);
                testInputData.Save(InputTestFilePath);
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}
