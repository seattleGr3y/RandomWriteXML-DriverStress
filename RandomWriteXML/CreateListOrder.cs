using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace RandomWriteXML
{
    internal static class CreateListOrder
    {
        internal static void RandomizeList(string dirName, string seedFilePath, bool randomize, string infIndexList, string startChoice, int executionCount, string supportFolderLOC, string InputTestFilePath, bool stopOnFail)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("RandomizeList starting...");
                Console.ForegroundColor = ConsoleColor.White;
                string InputTestFilePathBAK = dirName + @"\StressTestXML.xml.BAK";
                int infListCount = GetData.GetInfPathsList(Program.dirName).Count;
                Console.WriteLine("infListCount : " + infListCount);
                List<int> numbers = new List<int>(Enumerable.Range(1, infListCount));

                switch (randomize)
                {
                    case true:
                        if (infIndexList.Length.Equals(0))
                        {
                            numbers.Shuffle(infListCount);
                            infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                            File.WriteAllText(seedFilePath, infIndexList);
                            Array list = infIndexList.Split(',').Select(Int32.Parse).ToArray<int>();

                            foreach (int index in list)
                            {
                                Thread.Sleep(100);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("index per INF : " + index);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            XMLWriter.CreateXML(dirName, randomize, infIndexList, infIndexList, startChoice, executionCount, supportFolderLOC, InputTestFilePath);
                            Utilities.CopyFile(InputTestFilePath, InputTestFilePathBAK);
                            Thread.Sleep(2000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("...going to StartStress from TRUE next...");
                            Console.ForegroundColor = ConsoleColor.White;

                            DriverStressInit.StartStress(InputTestFilePath, Program.installer, Program.dirName, startChoice, Program.rollbackLine, infListCount = 0);
                        }
                        break;
                    case false:
                        infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                        File.WriteAllText(seedFilePath, infIndexList);
                        Console.WriteLine("we'll just continue as normal and NOT randomize the list");
                        XMLWriter.CreateXML(dirName, randomize, infIndexList, infIndexList, startChoice, executionCount, supportFolderLOC, InputTestFilePath);
                        Utilities.CopyFile(InputTestFilePath, InputTestFilePathBAK);
                        Thread.Sleep(2000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("...going to StartStress from FALSE next...");
                        Console.ForegroundColor = ConsoleColor.White;
                        DriverStressInit.StartStress(InputTestFilePath, Program.installer, Program.dirName, startChoice, Program.rollbackLine, infListCount = 0);
                        break;
                }
                ExecuteFromList.ExecuteTheList(randomize, executionCount, Program.dirName, InputTestFilePath, supportFolderLOC, seedFilePath, startChoice, stopOnFail);
            }

            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

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
