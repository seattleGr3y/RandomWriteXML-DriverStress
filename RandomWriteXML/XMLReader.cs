using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RandomWriteXML
{
    class XMLReader
    {
        internal static List<string> testInputData = new List<string>();
        internal static List<string> DriverNameList = new List<string>();
        internal static string InputTestFilePathBAK = @".\StressTestXML.xml.BAK";
        public static string DriverInfPath { get; private set; }

        /// <summary>
        /// get the list of driver paths for the app to use for execution
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static List<string> GetDriversPath(string InputTestFilePath)
        {
            try
            {
                var testInputData = XDocument.Load(InputTestFilePath);
                var driversPathList = testInputData.Descendants("InfDirectories");
                int infsCount = driversPathList.Count();
                if (infsCount.Equals(0))
                {
                    InputTestFilePath = InputTestFilePathBAK;
                    testInputData = XDocument.Load(InputTestFilePath);
                    driversPathList = testInputData.Descendants("InfDirectories");
                }

                foreach (var driverPath in driversPathList.Elements())
                {
                    string DriverInfPath = driverPath.Attribute("InfPath").Value;
                    XMLReader.testInputData.Add(DriverInfPath);
                }
                return XMLReader.testInputData;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                return null;
            }
        }

        /// <summary>
        /// get the list of driver paths for the app to use for execution
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string GetFriendlyDriverName(string InputTestFilePath, string line)
        {
            string friendlyDriverName = string.Empty;
            try
            {
                var testInputData = XDocument.Load(InputTestFilePath);
                var driversPathList = testInputData.Descendants("InfDirectories");
                int infsCount = driversPathList.Count();
                if (infsCount.Equals(0))
                {
                    InputTestFilePath = InputTestFilePathBAK;
                    testInputData = XDocument.Load(InputTestFilePath);
                    driversPathList = testInputData.Descendants("InfDirectories");
                }

                foreach (var driverPath in driversPathList.Elements())
                {
                    string DriverInfPath = driverPath.Attribute("InfPath").Value;
                    if (DriverInfPath.Equals(line))
                    {
                        friendlyDriverName = driverPath.Attribute("InfName").Value;
                    }
                }
                return friendlyDriverName;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                return null;
            }
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string FromINFIndex(string InputTestFilePath, int index)
        {
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePath);
                var driversPathList = testInputData.Descendants("InfDirectories");

                foreach (var driverPath in driversPathList.Elements())
                {
                    int testIndex = Convert.ToInt32(driverPath.Attribute("InfIndex").Value);
                    if (index.Equals(testIndex))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("testIndex : " + testIndex + " index : " + index);
                        string line = driverPath.Attribute("InfPath").Value;
                        Console.WriteLine("DriverInfPath : " + line);
                        Console.ForegroundColor = ConsoleColor.White;
                        result = line;
                    }
                    else { continue; }
                }
                Logger.FunctionLeave();
                return result;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                return null;
            }
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string IndexFromINF(string InputTestFilePath, string line)
        {
            string result = null;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePath);
                var driversPathList = testInputData.Descendants("InfDirectories");

                foreach (var driverPath in driversPathList.Elements())
                {
                    string testLine = driverPath.Attribute("InfPath").Value;
                    if (line.Equals(testLine))
                    {
                        string index = driverPath.Attribute("InfIndex").Value;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(" index : " + index);
                        Console.WriteLine("DriverInfPath : " + line);
                        Console.ForegroundColor = ConsoleColor.White;
                        result = index;
                    }
                    else { continue; }
                }
                Logger.FunctionLeave();
                return result;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                return result;
            }
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string IndexListFromINFs(string InputTestFilePathBAK)
        {
            string indexList = string.Empty;
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                var driversPathList = testInputData.Descendants("InfDirectories");

                foreach (var driverPath in driversPathList.Elements())
                {
                    string testLine = driverPath.Attribute("InfPath").Value;
                    string index = driverPath.Attribute("InfIndex").Value;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(" index : " + index);
                    Console.WriteLine("DriverInfPath : " + driverPath);
                    Console.ForegroundColor = ConsoleColor.White;
                    indexList = string.Join(index, ",");
                    result = indexList;
                }
                Logger.FunctionLeave();
                return result;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                return result;
            }
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static int GetExecutionCount(string InputTestFilePath)
        {
            try
            {
                Logger.FunctionEnter();
                int executionCount;
                var testInputData = XDocument.Load(InputTestFilePath);
                int testRunLoop = int.Parse(testInputData.XPathSelectElement("/Tests/TestChoices/ExecutionCount").Value);
                executionCount = testRunLoop;
                Logger.Comment("current executionCount from GetExecutionCount method : " + executionCount);
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
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string GetSeed(string InputTestFilePathBAK)
        {
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                string varIndexList = testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value.ToString();
                //List<string> listSeed = new List<string>();
                // listSeed = testInputData.XPathSelectElement("/DriverTests/Test/Seed").Value.ToList<string>;
                Logger.Comment("randomized list to save for re-use if need be : " + varIndexList);
                Logger.FunctionLeave();
                result = varIndexList;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            return result;
        }
    }
}
