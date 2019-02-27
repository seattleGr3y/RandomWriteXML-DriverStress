using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;

namespace DriverCapsuleStressTool
{
    class DriverStressInit
    {
        #region STRINGS AND THINGS
        internal static bool rebootRequired = false;
        internal static string infName = string.Empty;
        internal static string expectedDriverVersion = string.Empty;
        internal static string installArgs = string.Empty;
        internal static bool needRollBack = false;
        internal static Random rnd = new Random();
        #endregion

        /// <summary>
        /// Where most of the work is done to decide what to do then call the methods to do so
        /// </summary>
        /// <param name="installer"></param>
        /// <param name=""></param>
        /// <param name="dirName"></param>
        /// <param name="startChoice"></param>
        internal static void StartStress(string InputTestFilePath, string installer, string dirName, string startChoice, string rollbackLine, int infListCount = 0)
        {
            try
            {
                Logger.FunctionEnter();

                if (!File.Exists(dirName + @"\debugEnabled.txt"))
                {
                    Logger.Comment("enabled WinDebugMode we must reboot...");
                    Thread.Sleep(1000);
                    RebootAndContinue.EnableWinDebugMode();
                }
                if (RegCheck.IsRebootPending())
                {
                    Logger.Comment("there is a pending reboot...");
                    Thread.Sleep(3000);
                    RebootAndContinue.RebootCmd(true);
                }
                if (GetData.CheckCrashDumpOccurred())
                {
                    Logger.Comment("Looks like we found a crashdump check it out after reboot...");
                    Thread.Sleep(1000);
                    RebootAndContinue.RebootCmd(true);
                }

                GetData.CreateIfMissing(Program.resultsLogDir);
                int executionCount = XMLReader.GetExecutionCount(InputTestFilePath);

                string InputTestFilePathBAK = dirName + @"\DriverCapsuleStress.xml.BAK";
                if (!File.Exists(InputTestFilePathBAK))
                {
                    Utilities.CopyFile(InputTestFilePath, InputTestFilePathBAK);
                }

                infListCount = XMLReader.GetInfsPathListCount(Program.InputTestFilePathBAK);

                string infIndexListString = XMLReader.GetSeed(Program.InputTestFilePathBAK);
                if (string.IsNullOrEmpty(infIndexListString))
                {
                    RewriteXMLContinue(executionCount, infListCount);
                }
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// when we have run through the list this will reset the XML so it will start the list
        /// again and ensure the execution count is correct so testing will eventually complete
        /// </summary>
        /// <param name="executionCount"></param>
        /// <param name="infListCount"></param>
        internal static void RewriteXMLContinue(int executionCount, int infListCount)
        {
            try
            {
                XMLWriter.DecrementExecutionCount(Program.InputTestFilePathBAK, executionCount);
                Logger.Comment("executionCount after going thru all the loops : " + executionCount);
                File.Delete(Program.InputTestFilePath);
                var numbers = new List<int>(Enumerable.Range(1, infListCount));

                string infIndexList = string.Empty;
                // remove existing data in startSeed and currentSeed from .BAK file before copy
                bool randomize = GetData.GetRandomChoice(Program.InputTestFilePathBAK);
                if (randomize.Equals(true))
                {
                    numbers.Shuffle(infListCount);
                    infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                    File.WriteAllText(Program.seedFilePath, infIndexList);

                    XMLWriter.SaveSeed(Program.InputTestFilePathBAK, infIndexList, infIndexList);
                    Utilities.CopyFile(Program.InputTestFilePathBAK, Program.InputTestFilePath);
                    //Thread.Sleep(1000);
                }
                else
                {
                    infIndexList = string.Join(",", numbers.GetRange(0, infListCount));
                    File.WriteAllText(Program.seedFilePath, infIndexList);

                    XMLWriter.SaveSeed(Program.InputTestFilePathBAK, infIndexList, infIndexList);
                    Utilities.CopyFile(Program.InputTestFilePathBAK, Program.InputTestFilePath);
                    Logger.Comment("re-add the reg key to start post reboot...");
                    //Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
    }
}