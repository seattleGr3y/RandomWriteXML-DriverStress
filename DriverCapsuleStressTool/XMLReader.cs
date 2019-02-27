using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DriverCapsuleStressTool
{
    class XMLReader
    {
        internal static List<string> testInputData = new List<string>();
        internal static List<string> DriverNameList = new List<string>();
        internal static string InputTestFilePathBAK = @".\DriverCapsuleStress.xml.BAK";
        
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
                int infsCount = GetInfsPathListCount(Program.InputTestFilePathBAK);
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
        internal static string FromINFIndex(int infListCount, string InputTestFilePath, int index, int executionCount)
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
                        string line = driverPath.Attribute("InfPath").Value;
                        result = line;
                    }
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
                    string testLine = driverPath.Attribute("InfPath").Value.ToLower();
                    //Console.WriteLine("IndexFromINF finds testLine : " + testLine);
                    //Console.ReadKey();
                    if (line.Equals(testLine))
                    {
                        string index = driverPath.Attribute("InfIndex").Value;
                        //Console.WriteLine("IndexFromINF finds InfIndex : " + index);
                        //Console.ReadKey();
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
        /// read the XML to find the remaining indexes for remaining INFs to install to create a list to check what is 
        /// left so we choose existing paths from the XML to attempt to install if anything else goes wrong
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
                if (string.IsNullOrEmpty(testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value.ToString()))
                {
                    result = null;
                }
                else
                {
                    string varIndexList = testInputData.XPathSelectElement("/Tests/TestChoices/CurrentSeed").Value.ToString();
                    Logger.Comment("randomized list to save for re-use if need be : " + varIndexList);
                    Logger.FunctionLeave();
                    result = varIndexList;
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            return result;
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// XMLReader.GetLastInstalled();
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string GetLastInstalled()
        {
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(Program.InputTestFilePathBAK);
                string lastInstalled = testInputData.XPathSelectElement("/Tests/TestChoices/LastInstalled").Value.ToString();
                result = lastInstalled;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            return result;
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string GetStartSeed(string InputTestFilePathBAK)
        {
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                string varIndexList = testInputData.XPathSelectElement("/Tests/TestChoices/StartSeed").Value.ToString();
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
        
        internal static bool GetGroupFirmware(string InputTestFilePathBAK)
        {
            bool result = false;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                string groupFirmwareSTR = testInputData.XPathSelectElement("/Tests/TestChoices/GroupFirmware").Value.ToString();
                Logger.Comment("Should we install all firmware then reboot : " + groupFirmwareSTR);
                bool groupFirmware = Convert.ToBoolean(groupFirmwareSTR);
                Logger.FunctionLeave();
                result = groupFirmware;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            return result;
        }

        /// <summary>
        /// get the boolean user choice to stop on error or not
        /// </summary>
        /// XMLReader.GetStopOnError(InputTestFilePathBAK);
        /// <param name="InputTestFilePathBAK"></param>
        /// <returns></returns>
        internal static bool GetStopOnError(string InputTestFilePathBAK) 
        {
            bool result = false;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                string stopOnErrorSTR = testInputData.XPathSelectElement("/Tests/TestChoices/StopOnError").Value.ToString();
                Logger.Comment("Should we install all firmware then reboot : " + stopOnErrorSTR);
                bool stopOnError = Convert.ToBoolean(stopOnErrorSTR); 
                Logger.FunctionLeave();
                result = stopOnError;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            return result;
        }

        /// <summary>
        /// read the XML to find the number of times it will need to loop through the list
        /// infsPathListCount = GetInfsPathListCount(InputTestFilePathBAK);
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static int GetInfsPathListCount(string InputTestFilePathBAK)
        {
            int result = 0;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePathBAK);
                string infsPathListCountSTR = testInputData.XPathSelectElement("/Tests/TestChoices/InfsPathListCount").Value.ToString();
                int infsPathListCount = Convert.ToInt32(infsPathListCountSTR);
                Logger.Comment("count of inf paths to install + uninstall : " + infsPathListCount);
                Logger.FunctionLeave();
                result = infsPathListCount;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            return result;
        }
    }
}
