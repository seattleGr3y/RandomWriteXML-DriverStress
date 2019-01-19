using System;
using System.IO;



namespace DriverCapsuleStressTool
{
    class ParseForResults
    {
        internal static string errorCode = "Error code ";
        internal static string successUninstallResults = "Successfully uninstalled";
        internal static string successinstallResults = "Installation was successful.";
        internal static string failedResults = "Falied to install ";
        internal static int errorCount = 0;
        internal static int successInstallCount = 0; 
        internal static int successUninstallCount = 0; 
        internal static int failedCount = 0;
        
        /// <summary>
        /// logPath will be the location or the DPINST.LOG
        /// </summary>
        /// ParseForResults.ParseFromdpinstLog(logPath);
        /// <param name="logPath"></param>
        internal static void ParseFromdpinstLog(string logPath)
        {
            string dumpExist = "False";
            using (FileStream fs = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains(errorCode))
                    {
                        errorCount++;
                        Console.WriteLine("error here : " + line);
                    }

                    if (line.Contains(successUninstallResults))
                    {
                        successUninstallCount++;
                        Console.WriteLine("successUninstallCount here : " + line);
                    }

                    if (line.Contains(successinstallResults))
                    {
                        successInstallCount++;
                        Console.WriteLine("successInstallCount here : " + line);
                    }

                    if (line.Contains(failedResults))
                    {
                        failedCount++;
                        Console.WriteLine("failedResults here : " + line);
                    }
                }

                Console.WriteLine("failedCount : " + failedCount);
                Console.WriteLine("successinstallCount : " + successInstallCount);
                Console.WriteLine("successUninstallCount : " + successUninstallCount);
                Console.WriteLine("errorCount  : " + errorCount);
            }

            foreach (string dmpFileTest in Directory.EnumerateDirectories(Program.dirName))
            {
                if (dmpFileTest.Contains(".dmp"))
                {
                    dumpExist = "True";
                }
                else { continue; }
            }
            XMLWriter.LogResults(Program.InputTestFilePathBAK, errorCount.ToString(), failedCount.ToString(), successInstallCount.ToString(), successUninstallCount.ToString(), dumpExist);
        }
    }
}