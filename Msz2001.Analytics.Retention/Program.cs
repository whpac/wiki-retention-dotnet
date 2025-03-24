using System.Globalization;
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
                Console.WriteLine("Usage: Msz2001.Analytics.Retention <wikiDB> <outputDir>");
                Console.WriteLine();
                Console.WriteLine("  wikiDB     - name of the wiki database to process");
                Console.WriteLine("  outputDir  - where to save the result files");
                Console.WriteLine();
                return 1;
            }

            var wikiDB = args[0];
            var resultDir = args[1];

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var logReaderFactory = new LogDumpReaderFactory(loggerFactory, @"D:\dumps");
            var logReader = logReaderFactory.CreateReader(wikiDB);
            var blockedUsersProcessor = new BlockedUsersProcessor(logReader, loggerFactory);
            var blockedUsers = blockedUsersProcessor.Process();

            var historyReaderFactory = new HistoryDumpReaderFactory(loggerFactory, @"D:\dumps");
            var historyReader = historyReaderFactory.CreateReader(wikiDB);
            var userEditsProcessor = new UserEditsProcessor(historyReader, loggerFactory);
            var userDatas = userEditsProcessor.Process();

            // Checks ensure that two infinite blocks don't overflow the counter
            static TimeSpan addTimeSpans(TimeSpan t1, TimeSpan t2) =>
                t1 == TimeSpan.MaxValue || t2 == TimeSpan.MaxValue ? TimeSpan.MaxValue :
                t1 + t2;

            foreach (var (_, data) in userDatas)
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

            var rawOutputFile = Path.Combine(resultDir, wikiDB + ".tsv");
            UserDataWriter.Write(rawOutputFile, userDatas);

            var classifiers = new Dictionary<string, IClassifier>
            {
                { "wmf", new WmfClassifier() },
                { "msz", new MszClassifier() }
            };

            foreach (var (key, classifier) in classifiers)
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

            return 0;
        }
    }
}
