﻿using System.Globalization;
using System.Numerics;

using Microsoft.Extensions.Logging;

using Msz2001.Analytics.Retention.Classifiers;
using Msz2001.Analytics.Retention.Processors;
using Msz2001.Analytics.Retention.Writers;
using Msz2001.MediaWikiDump.HistoryDumpClient.Toolforge;
using Msz2001.MediaWikiDump.XmlDumpClient.Toolforge;

namespace Msz2001.Analytics.Retention
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: Msz2001.Analytics.Retention <wikiDB> <outputDir> [maxYear]");
                Console.WriteLine();
                Console.WriteLine("  wikiDB     - name of the wiki database to process");
                Console.WriteLine("  outputDir  - where to save the result files");
                Console.WriteLine("  maxYear    - optional, the maximum year to process (default: current)");
                Console.WriteLine();
                return 1;
            }

            var wikiDB = args[0];
            var resultDir = args[1];
            var maxYear = DateTime.Now.Year;
            if (args.Length > 2)
            {
                if (!int.TryParse(args[2], out maxYear))
                {
                    Console.WriteLine($"Invalid year: {args[2]}");
                    return 1;
                }
                else
                {
                    Console.WriteLine($"Processing data up to year {maxYear}");
                }
            }

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();

            var logReaderFactory = new LogDumpReaderFactory(loggerFactory);
            var logReader = logReaderFactory.CreateReader(wikiDB);
            var blockedUsersProcessor = new BlockedUsersProcessor(logReader, loggerFactory);
            var blockedUsers = blockedUsersProcessor.Process();

            var historyReaderFactory = new HistoryDumpReaderFactory(loggerFactory);
            var historyReader = historyReaderFactory.CreateReader(wikiDB);
            var userEditsProcessor = new UserEditsProcessor(historyReader, loggerFactory)
            {
                MaxYear = maxYear
            };
            var userDatas = userEditsProcessor.Process();

            // Checks ensure that two infinite blocks don't overflow the counter
            static TimeSpan addTimeSpans(TimeSpan t1, TimeSpan t2) =>
                t1 == TimeSpan.MaxValue || t2 == TimeSpan.MaxValue ? TimeSpan.MaxValue :
                t1 + t2;

            foreach (var (_, data) in userDatas)
            {
                try
                {
                    if (blockedUsers.TryGetValue(data.UserName, out var blocks))
                    {
                        data.BlockDuration = (from block in blocks
                                              where block.Start <= data.FirstEditPlus2Months
                                                || data.FirstEditPlus2Months is null
                                              select block.Duration)
                                             .Aggregate(TimeSpan.Zero, addTimeSpans);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to process blocks for user {UserName}", data.UserName);
                }
            }

            var rawOutputFile = Path.Combine(resultDir, wikiDB + ".tsv");
            UserDataWriter.Write(rawOutputFile, userDatas);

            var classifiers = new Dictionary<string, IClassifier>
            {
                { "wmf", new WmfClassifier() },
                { "msz", new MszClassifier() },
                { "cvs", new CvsClassifier() }
            };

            foreach (var (key, classifier) in classifiers)
            {
                try
                {
                    var classifiedFile = Path.Combine(resultDir, $"{wikiDB}.{key}.tsv");
                    var monthlyCounts = new Dictionary<string, Dictionary<string, uint>>();

                    foreach (var user in userDatas.Values)
                    {
                        var userClass = classifier.Classify(user);
                        var userMonth = user.GetBaselineDate()?.ToString("yyyy-MM");
                        if (userMonth is null)
                            continue;

                        if (!monthlyCounts.TryGetValue(userMonth, out var classCounts))
                        {
                            monthlyCounts[userMonth] = classCounts =
                                classifier.Classes.ToDictionary(className => className, _ => 0u);
                        }
                        classCounts[userClass]++;
                    }

                    ClassWriter.Write(classifiedFile, classifier.Classes, monthlyCounts);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to classify users with `{Classifier}`", key);
                }
            }

            return 0;
        }
    }
}
