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
            string lineContains = string.Empty;
            try
            {
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
                                switch (lineContains)
                                {
                                    case string errorResult when line.Contains(errorCode):
                                        errorCount++;
                                        break;
                                    case string successUninstallResult when line.Contains(successUninstallResults):
                                        successUninstallCount++;
                                        break;
                                    case string failedResult when line.Contains(failedResults):
                                        failedCount++;
                                        break;
                                    case string successinstallResult when line.Contains(successinstallResults):
                                        successInstallCount++;
                                        break;
                                }
                            }
                        }

                        if (line.Contains(rollbackCheck))
                        {
                            switch (lineContains)
                            {
                                case string errorResult when line.Contains(errorCode):
                                    rollBackErrorCount++;
                                    break;
                                case string successinstallResult when line.Contains(successinstallResults):
                                    rollBackSuccessCount++;
                                    break;
                                case string failedResult when line.Contains(failedResults):
                                    rollBackFailedCount++;
                                    break;
                            }
                        }
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

            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// might be writing this to the XML for logging
        /// </summary>
        /// <param name="capsuleReturnCode"></param>
        internal static void GetErrorsFromMyLog(int capsuleReturnCode)
        {
            string line = string.Empty;
            using (FileStream fs = File.Open(Program.resultsLogDir, FileMode.Open, FileAccess.Read, FileShare.Inheritable))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("failed to install due to "))
                    {
                        switch (capsuleReturnCode)
                        {
                            case 1:
                                Console.WriteLine("this failed straight-up registry code = 1");
                                break;

                            case 2:
                                Console.WriteLine("this failed straight-up registry code = 2");
                                break;

                            case 3:
                                Console.WriteLine("this failed straight-up registry code = 3");
                                break;

                            case 4:
                                Console.WriteLine("this failed straight-up registry code = 4");
                                break;

                            case 5:
                                Console.WriteLine("this failed straight-up registry code = 5");
                                break;

                            case 6:
                                Console.WriteLine("this failed straight-up registry code = 6");
                                break;

                            case 7:
                                Console.WriteLine("this failed straight-up registry code = 7");
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}