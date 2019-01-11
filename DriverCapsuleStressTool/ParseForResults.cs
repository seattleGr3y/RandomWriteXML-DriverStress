using System;
using Microsoft.HWSW.Test.Utilities;
using System.IO;

namespace DriverCapsuleStressTool
{
    class ParseForResults
    {
        internal static string errorCode = "Error code ";
        internal static string passedResults = "Successfully installed ";
        internal static string failedResults = "Falied to install ";
        internal static int errorCount = 0;
        internal static int passedCount = 0;
        internal static int failedCount = 0;

        /// <summary>
        /// logPath will be the location or the DPINST.LOG
        /// </summary>
        /// ParseForResults.ParseFromdpinstLog(logPath);
        /// <param name="logPath"></param>
        internal static void ParseFromdpinstLog(string logPath)
        {
            string dumpExist;
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

                    if (line.Contains(passedResults))
                    {
                        passedCount++;
                        Console.WriteLine("passedResults here : " + line);
                    }

                    if (line.Contains(failedResults))
                    {
                        failedCount++;
                        Console.WriteLine("failedResults here : " + line);
                    }
                }
                Console.WriteLine("failedCount : " + failedCount);
                Console.WriteLine("passedCount : " + passedCount);
                Console.WriteLine("errorCount  : " + errorCount);
            }
            foreach (string dmpFileTest in Directory.EnumerateDirectories(Program.dirName))
            {
                if (dmpFileTest.Contains(".dmp"))
                {
                    dumpExist = "true";
                    XMLWriter.LogResults(Program.InputTestFilePathBAK, errorCount.ToString(), failedCount.ToString(), passedCount.ToString(), dumpExist);
                }
            }
        }
    }
}
