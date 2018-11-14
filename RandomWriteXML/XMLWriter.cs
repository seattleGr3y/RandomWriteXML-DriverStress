using Microsoft.HWSW.Test.Utilities;
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
            
            foreach (string infDir in GetData.GetInfPathsList(supportFolderLOC))
            {
                //GetData.GetInfPathsList(supportFolderLOC);
                string infName = Path.GetFileName(infDir);
                //string infName = Directory.GetFiles(infDir, ".inf").ToString();
                infIndex++;
                xmlWriter.WriteStartElement("InfDir");
                xmlWriter.WriteAttributeString("InfName", infName); // infDir.Split('\\')[5]);
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
            //Console.ReadKey();
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
                //Utilities.CopyFile(InputTestFilePathBAK, InputTestFilePath);
                //List<string> listSeed = new List<string>();
                // listSeed = testInputData.XPathSelectElement("/DriverTests/Test/Seed").Value.ToList<string>;
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

        internal static void SaveCurrentSeed(string InputTestFilePath, string InputTestFilePathBAK, string currentSeed)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = currentSeed;
                testInputData.Save(InputTestFilePathBAK);
                var testInputData2 = XDocument.Load(InputTestFilePath);
                testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value = currentSeed;
                testInputData.Save(InputTestFilePath);

                Logger.Comment("randomized list to save for re-use if need be : " + currentSeed);
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
                //int testRunLoop = int.Parse(testInputData.XPathSelectElement("/Tests/TestChoices/ExecutionCount").Value);
                //testRunLoop--;
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
                Thread.Sleep(1000);
                testInputData.Save(InputTestFilePath);
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}
