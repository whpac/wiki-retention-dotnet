using Microsoft.Extensions.Logging;

using Msz2001.Analytics.Retention.Instrumentation;
using Msz2001.Analytics.Retention.Utils;
using Msz2001.MediaWikiDump.XmlDumpClient.Entities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Extractors
{
    internal partial class ListBlocks(StreamWriter OutputStream, ILogger logger) : IExtractor<LogItem>
    {
        private static readonly HashSet<string> INFINITE_DESIGNATORS = ["infinite", "indefinite", "infinity", "never"];

        public string Name => nameof(RegistrationDates);
        private readonly ILogger Logger = logger;

        public void BeforeProcessing()
        {
            WriteOutputRecord("user_name", "timestamp", "block_duration", "block_end", "log_id", "method");
        }

        public void ProcessEntry(LogItem entry)
        {
            if (entry.Type != "block")
                return;

            if (entry.User is null || entry.User.Id is null || entry.Title is null)
                return;

            var blockedUserName = entry.Title.PageName;

            string blockDuration = "";
            Timestamp? blockEndDate = null;
            if (entry.Params is null || entry.Params as string == "")
            {
                blockDuration = "";
            }
            else if (entry.Params is string paramsStr)
            {
                blockDuration = paramsStr.Split('\n')[0];
            }
            else if (entry.Params is Dictionary<object, object?> paramsDict)
            {
                blockDuration = paramsDict.GetValueOrDefault("5::duration") as string ?? "";
            }
            else
            {
                LogNoDuration(Logger, entry.Params?.ToString() ?? "", entry.Id);
            }

            if (blockDuration == "") { }
            else if (INFINITE_DESIGNATORS.Contains(blockDuration))
            {
                blockEndDate = Timestamp.Infinite;
            }
            else if (
                DateTime.TryParse(
                    blockDuration,
                    CultureInfo.InvariantCulture,
                    out DateTime endDate
                )
            ) {
                blockEndDate = new Timestamp(endDate);
            }
            else
            {
                blockEndDate = entry.Timestamp + RelativeTimeParser.ParseRelativeTime(blockDuration);
            }

            WriteOutputRecord(
                blockedUserName,
                entry.Timestamp.ToString(),
                blockDuration,
                blockEndDate?.ToString() ?? "",
                entry.Id,
                entry.Action
            );
        }

        public void FinishProcessing()
        {
            OutputStream.Flush();
        }

        protected void WriteOutputRecord(params object[] columns)
        {
            OutputStream.WriteLine(string.Join("\t", columns));
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = "Can't extract block duration from `{ParamsString}` for entry {EntryId}")]
        static partial void LogNoDuration(ILogger logger, string paramsString, uint entryId);
    }
}
