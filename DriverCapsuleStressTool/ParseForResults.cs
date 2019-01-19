using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace DriverCapsuleStressTool
{
    class ParseForResults
    {
        internal static string errorCode = "Error code ";
        internal static string successUninstallResults = "Successfully uninstalled ";
        internal static string successinstallResults = "Successfull installation of ";
        internal static string failedResults = "Failed to install ";
        internal static int errorCount = 0;
        internal static int successInstallCount = 0;
        internal static int successUninstallCount = 0;
        internal static int failedCount = 0;
        internal static string rollbackCheck = @"\rollbacks\";
        internal static int rollBackErrorCount = 0;
        internal static int rollBackSuccessCount = 0;
        internal static int rollBackFailedCount = 0; 

        /// <summary>
        /// logPath will be the location or the DPINST.LOG
        /// </summary>
        /// ParseForResults.ParseFromdpinstLog(logPath);
        /// <param name="logPath"></param>
        internal static void ParseFromdpinstLog(string logPath)
        {
            List<string> infsPathsList = GetData.GetInfPathsList(Program.dirName);
            string line = string.Empty;
            string dumpExist = string.Empty;
            string logString = string.Empty;
            using (FileStream fs = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.Inheritable))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                while ((line = sr.ReadLine()) != null)  
                {
                    foreach (string infPathTMP in infsPathsList)
                    {
                        string inNameTMP = Path.GetFileNameWithoutExtension(infPathTMP).ToLower();
                        if (line.Contains(inNameTMP.ToLower()))
                        {
                            if (line.Contains(errorCode)) // & line.Contains(infName))
                            {
                                errorCount++;
                                //Logger.Comment("error here : " + line);
                                //Console.WriteLine("error here : " + line);
                            }
                            
                            if (line.Contains(successUninstallResults)) // & line.Contains(infName))
                            {
                                successUninstallCount++;
                                //Logger.Comment("successUninstallCount here : " + line);
                                //Console.WriteLine("successUninstallCount here : " + line);
                            }

                            if (line.Contains(successinstallResults)) // & line.Contains(infName))
                            {
                                successInstallCount++;
                                //Logger.Comment("successInstallCount here : " + line);
                                //Console.WriteLine("successInstallCount here : " + line);
                            }

                            if (line.Contains(failedResults)) // & line.Contains(infName))
                            {
                                failedCount++;
                                //Logger.Comment("failedResults here : " + line);
                                //Console.WriteLine("failedResults here : " + line);
                            }
                        }
                        XMLWriter.LogResults(Program.InputTestFilePathBAK, errorCount.ToString(), failedCount.ToString(), successInstallCount.ToString(), successUninstallCount.ToString(),
                            rollBackErrorCount.ToString(), rollBackFailedCount.ToString(), rollBackSuccessCount.ToString(), dumpExist, logString);
                    }

                    if (line.Contains(rollbackCheck))
                    {
                        if (line.Contains(errorCode)) // & line.Contains(infName))
                        {
                            rollBackErrorCount++;
                            //Logger.Comment("rollback error here : " + line);
                            //Console.WriteLine("rollback error here : " + line);
                        }

                        if (line.Contains(successinstallResults)) // & line.Contains(infName))
                        {
                            rollBackSuccessCount++;
                            //Logger.Comment("rollback successInstallCount here : " + line);
                            //Console.WriteLine("rollback successInstallCount here : " + line);
                        }

                        if (line.Contains(failedResults)) // & line.Contains(infName))
                        {
                            rollBackFailedCount++;
                            //Logger.Comment("rollback failedResults here : " + line);
                            //Console.WriteLine("rollback failedResults here : " + line);
                        }
                    }
                    XMLWriter.LogResults(Program.InputTestFilePathBAK, errorCount.ToString(), failedCount.ToString(), successInstallCount.ToString(), successUninstallCount.ToString(),
                        rollBackErrorCount.ToString(), rollBackFailedCount.ToString(), rollBackSuccessCount.ToString(), dumpExist, logString);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("failedCount : " + failedCount);
                Console.WriteLine("successInstallCount : " + successInstallCount);
                Console.WriteLine("successUninstallCount : " + successUninstallCount);
                Console.WriteLine("errorCount  : " + errorCount);
                Console.WriteLine("rollBackFailedCount : " + rollBackFailedCount);
                Console.WriteLine("rollBackSuccessInstallCount : " + rollBackSuccessCount);
                Console.WriteLine("rollBackErrorCount  : " + rollBackErrorCount);
                Console.ForegroundColor = ConsoleColor.White;
            }

            foreach (string dmpFileTest in Directory.EnumerateDirectories(Program.dirName))
            {
                if (File.Exists(dmpFileTest.Contains(".dmp").ToString()))
                {
                    dumpExist = "True";
                }
                else { continue; }
            }
            XMLWriter.LogResults(Program.InputTestFilePathBAK, errorCount.ToString(), failedCount.ToString(), successInstallCount.ToString(), successUninstallCount.ToString(),
                rollBackErrorCount.ToString(), rollBackFailedCount.ToString(), rollBackSuccessCount.ToString(), dumpExist, logString);
        }
    }
}