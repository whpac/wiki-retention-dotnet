using System.Globalization;
using System.Numerics;

using Microsoft.Extensions.Logging;

using Msz2001.Analytics.Retention.Classifiers;
using Msz2001.Analytics.Retention.Processors;

namespace Msz2001.Analytics.Retention
{
    internal class Program2
    {
        static void Main(string[] args)
        {
            var wikiDB = "plwikinews";
            var blockLogFile = @$"D:\dumps\{wikiDB}-20250301-pages-logging.xml.gz";
            var rawOutputFile = @$"D:\{wikiDB}.tsv";
            var classFileTemplate = @$"D:\{wikiDB}.%sgn%.tsv";

            var logger = ConfigureLogger();
            using var blockedUsersProcessor = new BlockedUsersProcessor(blockLogFile, logger);
            var blockedUsers = blockedUsersProcessor.Process();

            var userEditsProcessor = new UserEditsProcessor(wikiDB, logger);
            var userDatas = userEditsProcessor.Process();

            foreach (var (_, data) in userDatas)
            {
                if (blockedUsers.TryGetValue(data.UserName, out var blocks))
                {
                    data.BlockDuration = (from block in blocks
                                          where block.Start <= data.RegistrationPlus2Months
                                            || data.RegistrationPlus2Months is null
                                          select block.Duration)
                                         .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
                }
            }

            using var rawFileStream = File.OpenWrite(rawOutputFile);
            using var rawWriter = new StreamWriter(rawFileStream);

            WriteCells(
                rawWriter,
                "UserId",
                "UserName",
                "RegistrationDate",
                "FirstEditDate",
                "TotalEdits",
                "Edits_reg1d",
                "Edits_regm2",
                "Edits_1em2",
                "IsBot",
                "IsCrossWiki",
                "BlockDays"
            );
            
            foreach (var (userId, userData) in userDatas)
            {
                WriteCells(
                    rawWriter,
                    userId,
                    userData.UserName,
                    TimestampToString(userData.RegistrationDate),
                    TimestampToString(userData.FirstEditDate),
                    userData.TotalEdits,
                    userData.Edits_reg1d,
                    userData.Edits_regm2,
                    userData.Edits_1em2,
                    userData.IsBot,
                    userData.IsCrossWiki,
                    userData.BlockDays.ToString("F3")
                );
            }

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
                        classCounts = [];
                        foreach (var className in classifier.Classes)
                        {
                            classCounts[className] = 0;
                        }
                        monthlyCounts[userMonth] = classCounts;
                    }
                    classCounts[userClass]++;
                }

                using var classFileStream = File.Open(classifiedFile, FileMode.Create, FileAccess.Write);
                using var classWriter = new StreamWriter(classFileStream);

                classWriter.Write("Month");
                foreach (var className in classifier.Classes)
                {
                    classWriter.Write("\t" + className);
                }
                classWriter.WriteLine();
                foreach (var (month, userClasses) in monthlyCounts.OrderBy(e => e.Key))
                {
                    classWriter.Write(month);
                    foreach (var className in classifier.Classes)
                    {
                        classWriter.Write("\t" + userClasses[className]);
                    }
                    classWriter.WriteLine();
                }
            }
        }

        static ILogger ConfigureLogger()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            return loggerFactory.CreateLogger<Program2>();
        }

        private static void WriteCells(StreamWriter writer, params object[] columns)
        {
            writer.WriteLine(string.Join("\t", columns));
        }

        private static string TimestampToString(DateTime? timestamp)
        {
            return timestamp?.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture) ?? "";
        }
    }
}
