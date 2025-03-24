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
        static void Main(string[] args)
        {
            var wikiDB = "plwikinews";
            var rawOutputFile = @$"D:\{wikiDB}.tsv";
            var classFileTemplate = @$"D:\{wikiDB}.%sgn%.tsv";

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var logReaderFactory = new LogDumpReaderFactory(loggerFactory, @"D:\dumps");
            var logReader = logReaderFactory.CreateReader(wikiDB);
            var blockedUsersProcessor = new BlockedUsersProcessor(logReader, loggerFactory);
            var blockedUsers = blockedUsersProcessor.Process();

            var historyReaderFactory = new HistoryDumpReaderFactory(loggerFactory, @"D:\dumps");
            var historyReader = historyReaderFactory.CreateReader(wikiDB);
            var userEditsProcessor = new UserEditsProcessor(historyReader, loggerFactory);
            var userDatas = userEditsProcessor.Process();

            foreach (var (_, data) in userDatas)
            {
                if (blockedUsers.TryGetValue(data.UserName, out var blocks))
                {
                    data.BlockDuration = (from block in blocks
                                          where block.Start <= data.FirstEditPlus2Months
                                            || data.FirstEditPlus2Months is null
                                          select block.Duration)
                                         .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
                }
            }

            UserDataWriter.Write(rawOutputFile, userDatas);

            var classifiers = new Dictionary<string, IClassifier>
            {
                { "wmf", new WmfClassifier() },
                { "msz", new MszClassifier() }
            };

            foreach (var (key, classifier) in classifiers)
            {
                var classifiedFile = classFileTemplate.Replace("%sgn%", key);
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
        }
    }
}
