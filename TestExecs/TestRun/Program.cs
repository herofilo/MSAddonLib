using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MSAddonLib.Domain;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Persistence;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;

namespace TestRun
{
    class Program
    {
        private static ProcessingFlags _processingFlags = ProcessingFlags.None;

        private static string _tempPath = null;

        private static string _diskEntity = null;

        private static MoviestormPaths _moviestormPaths = null;

        static void Main(string[] args)
        {
            string errorText;
            if (!InitializationChores(args, out errorText))
            {
                Console.WriteLine($"ERROR: {errorText}");
                return;
            }

            Console.WriteLine($"Temp path: {_tempPath}");
            Console.WriteLine($"Entity Path: {_diskEntity}");

            StringReportWriter writer = new StringReportWriter();
            IDiskEntity entity = DiskEntityHelper.GetEntity(_diskEntity, false, writer);
            Console.WriteLine($"Entity Type: {entity.DiskEntityType}");

            bool result = entity.CheckEntity(_processingFlags);
            Console.WriteLine($"CheckEntity() result: {result}");
            string output = entity.ToString();
            Console.WriteLine(writer.Text);
            Console.WriteLine("-------------");

            Console.Write("Press for finish");
            Console.ReadKey();
        }

        private static bool InitializationChores(string[] pArgs, out string pErrorText)
        {
            pErrorText = null;
            if (!CheckArguments(pArgs))
                return false;

            _tempPath = Utils.GetTempDirectory();

            string errorText;
            Utils.ResetTempFolder(out errorText);

            _moviestormPaths = AddonPersistenceUtils.GetMoviestormPaths(out errorText);
            if (_moviestormPaths == null)
            {
                pErrorText = errorText;
                return false;
            }

            return true;
        }

        private static bool CheckArguments(string[] pArgs)
        {
            if (pArgs.Length == 0)
                return false;

            _processingFlags = ProcessingFlags.None;
            foreach (string arg in pArgs)
            {
                if (arg.StartsWith("-"))
                {
                    string argLower = arg.ToLower();
                    if (argLower == "-showaddoncontents" || argLower == "-c")
                    {
                        _processingFlags |= ProcessingFlags.ShowAddonContents;
                        continue;
                    }
                    if (argLower == "-justreportissues" || argLower == "-i")
                    {
                        _processingFlags |= ProcessingFlags.JustReportIssues;
                        continue;
                    }
                    if (argLower == "-listallanimationfiles" || argLower == "-laa")
                    {
                        _processingFlags |= ProcessingFlags.ListAllAnimationFiles;
                        continue;
                    }
                    if (argLower == "-listgesturegaitsanimations" || argLower == "-lga")
                    {
                        _processingFlags |= ProcessingFlags.ListGestureGaitsAnimations;
                        continue;
                    }
                    if (argLower == "-listweirdgesturegaitsverbs" || argLower == "-lw")
                    {
                        _processingFlags |= ProcessingFlags.ListWeirdGestureGaitsVerbs;
                        continue;
                    }
                    if (argLower == "-listcompactdupverbsbyname" || argLower == "-lcdv")
                    {
                        _processingFlags |= ProcessingFlags.ListCompactDupVerbsByName;
                        continue;
                    }
                    if (argLower == "-correctdisguisedfiles" || argLower == "-cdf")
                    {
                        _processingFlags |= ProcessingFlags.CorrectDisguisedFiles;
                        continue;
                    }

                    continue;
                }

                _diskEntity = arg;
            }

            return _diskEntity != null;
        }
    }
}
