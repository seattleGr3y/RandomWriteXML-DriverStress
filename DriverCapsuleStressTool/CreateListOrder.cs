using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DriverCapsuleStressTool
{
    internal static class CreateListOrder
    {
        /// <summary>
        /// Setting up the tool to execute through the list of INFs randomly or in order
        /// </summary>
        /// <param name="randomizeList"></param>
        /// <param name="loopCount"></param>
        /// <param name="startChoice"></param>
        /// <param name="stopOnErrorSTR"></param>
        /// <param name="groupFirmwareSTR"></param>
        internal static void RandomizeList(string randomizeList, string loopCount, string startChoice, string stopOnErrorSTR, string groupFirmwareSTR, string custom)
        {
            Console.WriteLine("randomizeList = " + randomizeList);
            bool randomize = Convert.ToBoolean(randomizeList);
            int executionCount = Convert.ToInt32(loopCount);
            string dumpFilePath = string.Empty;
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("RandomizeList starting...");
                Console.ForegroundColor = ConsoleColor.White;
                List<string> infsPathList = GetData.GetInfPathsList(Program.dirName);
                int infListCount = infsPathList.Count;
                Console.WriteLine("infListCount : " + infListCount);
                List<int> numbers = new List<int>(Enumerable.Range(1, infListCount));

                switch (randomize)
                {
                    case true:
                        numbers.Shuffle(infListCount);
                        string infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                        File.WriteAllText(Program.seedFilePath + executionCount + ".txt", infIndexList);
                        Array list = infIndexList.Split(',').Select(Int32.Parse).ToArray<int>();

                        XMLWriter.CreateXML(Program.dirName, randomize, infIndexList, infIndexList, startChoice, executionCount, Program.supportFolderLOC, Program.InputTestFilePath, stopOnErrorSTR, groupFirmwareSTR, dumpFilePath, custom);
                        Utilities.CopyFile(Program.InputTestFilePath, Program.InputTestFilePathBAK);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("...going to StartStress from TRUE next...");
                        Console.ForegroundColor = ConsoleColor.White;

                        DriverStressInit.StartStress(Program.InputTestFilePath, Program.installer, Program.dirName, startChoice, Program.rollbackLine, infListCount = 0);

                        break;
                    case false:
                        infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                        File.WriteAllText(Program.seedFilePath + executionCount + ".txt", infIndexList);
                        Console.WriteLine("we'll just continue as normal and NOT randomize the list");
                        XMLWriter.CreateXML(Program.dirName, randomize, infIndexList, infIndexList, startChoice, executionCount, Program.supportFolderLOC, Program.InputTestFilePath, stopOnErrorSTR, groupFirmwareSTR, dumpFilePath, custom);
                        Utilities.CopyFile(Program.InputTestFilePath, Program.InputTestFilePathBAK);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("...going to StartStress from FALSE next...");
                        Console.ForegroundColor = ConsoleColor.White;
                        DriverStressInit.StartStress(Program.InputTestFilePath, Program.installer, Program.dirName, startChoice, Program.rollbackLine, infListCount = 0);
                        break;
                }
                ExecuteFromList.ExecuteTheList(randomize, executionCount, startChoice);
            }

            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// this will take the list of index's per INF to install and use a random #
        /// from that list as the random seed to shuffle the list so it will execute
        /// in a random order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="infListCount"></param>
        internal static void Shuffle<T>(this IList<T> list, int infListCount)
        {
            try
            {
                Random r = new Random();
                int Seed = r.Next(1, infListCount);
                int n = list.Count;
                if (infListCount <= 2) { Seed = infListCount; }
                Random rnd = new Random();

                while (n > 1)
                {
                    n--;
                    int k = rnd.Next(Seed);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}
