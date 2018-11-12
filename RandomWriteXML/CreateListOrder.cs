using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace RandomWriteXML
{
    internal static class CreateListOrder
    {
        internal static void RandomizeList(string dirName, string seedFilePath, bool randomize, string infIndexList, string startChoice, string capStressChoice, int executionCount, string supportFolderLOC, string InputTestFilePath)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("am i getting to here RandomizeList starting...?");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.ReadKey();

            string InputTestFilePathBAK = dirName + @"\StressTestXML.xml.BAK";
            //int driverPathListCount = Directory.GetDirectories(supportFolderLOC).Count();
            int infListCount = Directory.GetDirectories(supportFolderLOC).Count();
            switch (randomize)
            {
                case true:
                    if (infIndexList.Length.Equals(0))
                    {
                        var numbers = new List<int>(Enumerable.Range(1, infListCount));
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
                        XMLWriter.CreateXML(dirName, randomize, infIndexList, infIndexList, startChoice, capStressChoice, executionCount, supportFolderLOC, InputTestFilePath);
                        File.Copy(InputTestFilePath, InputTestFilePathBAK);
                        Thread.Sleep(5000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("am i getting to here ...going to StartStress from TRUE next...?");
                        Console.ForegroundColor = ConsoleColor.White;
                        //Console.ReadKey();
                        DriverStressInit.StartStress(InputTestFilePath, Program.installer, Program.dirName, startChoice, Program.rollbackLine, infListCount = 0);
                    }
                    break;
                case false:
                    Console.WriteLine("we'll just continue as normal and NOT randomize the list");
                    XMLWriter.CreateXML(dirName, randomize, infIndexList, infIndexList, startChoice, capStressChoice, executionCount, supportFolderLOC, InputTestFilePath);
                    File.Copy(InputTestFilePath, InputTestFilePathBAK);
                    Thread.Sleep(5000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("am i getting to here ...going to StartStress from FALSE next...?");
                    Console.ForegroundColor = ConsoleColor.White;
                    //Console.ReadKey();
                    DriverStressInit.StartStress(InputTestFilePath, Program.installer, Program.dirName, startChoice, Program.rollbackLine, infListCount = 0);
                    break;
            }

            ExecuteFromList.ExecuteTheList(randomize, executionCount, RandomWriteXML.Program.dirName, InputTestFilePath, supportFolderLOC, seedFilePath, startChoice);
        }

        internal static void Shuffle<T>(this IList<T> list, int infListCount)
        {
            int n = list.Count;
            int Seed = 8;
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
    }
}
