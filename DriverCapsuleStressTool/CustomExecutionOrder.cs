using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DriverCapsuleStressTool
{
    class CustomExecutionOrder
    {
        /// <summary>
        /// This option will allow the user to enter a comma seperate list of INF names
        /// to create a custom execution order to install\uninstall drivers+capsules
        /// at runtime will look like this DriverCapsuleStressTool.exe --custom SurfaceUEFI.inf,IntcAudioBus.inf,IntcOED.inf
        /// </summary>
        /// <param name="custom"></param>
        internal static List<string> CustomOrder(string custom)
        {
            File.WriteAllText(Program.customListFile, custom);
            string fullInfPath = string.Empty;
            List<string> orderToRun = new List<string>();
            List<string> infsPathList = new List<string>();
            orderToRun = custom.Split(',').ToList();
            foreach (string infPath in orderToRun)
            {
                foreach (string customInfPath in Directory.EnumerateFiles(Program.dirName, infPath, SearchOption.AllDirectories))
                {
                    if (customInfPath.Contains("rollbacks")) { continue; }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("this will be in the custom list : " + customInfPath);
                    Console.ForegroundColor = ConsoleColor.White;
                    infsPathList.Add(customInfPath);
                }
            }
            Console.ReadKey();
            return infsPathList;
        }
    }
}
