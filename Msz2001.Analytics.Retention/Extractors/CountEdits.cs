using Microsoft.Extensions.Logging;

using Msz2001.Analytics.Retention.Instrumentation;
using Msz2001.MediaWikiDump.XmlDumpClient.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Extractors
{
    internal partial class CountEdits(
        StreamWriter OutputStream, ILogger logger, Dictionary<uint, CountEdits.TimeBounds> bounds
    ) : IExtractor<Page>
    {
        public string Name => nameof(CountEdits);
        private readonly ILogger Logger = logger;
        private readonly Dictionary<uint, uint> UserEditCount = new();
        private readonly Dictionary<uint, TimeBounds> Bounds = bounds;

        public void BeforeProcessing() { }

        public void ProcessEntry(Page entry)
        {
            foreach (var rev in entry.Revisions)
            {
                if (rev.User is null || rev.User.Id is null) continue;

                if (!Bounds.TryGetValue(rev.User.Id.Value, out TimeBounds bounds))
                {
                    LogNoBounds(Logger, rev.User.Id.Value);
                    continue;
                }
                if (rev.Timestamp < bounds.Start || rev.Timestamp > bounds.End)
                {
                    continue;
                }

                if (!UserEditCount.TryGetValue(rev.User.Id.Value, out uint storedCount))
                {
                    UserEditCount[rev.User.Id.Value] = 1;
                }
                else
                {
                    UserEditCount[rev.User.Id.Value] = storedCount + 1;
                }
            }
        }

        public void FinishProcessing()
        {
            WriteOutputRecord("user_id", "num_edits");

            foreach (var ec in UserEditCount)
            {
                WriteOutputRecord(ec.Key, ec.Value);
            }

            OutputStream.Flush();
        }

        protected void WriteOutputRecord(params object[] columns)
        {
            OutputStream.WriteLine(string.Join("\t", columns));
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = "Can't find bounds for user {UserId}")]
        static partial void LogNoBounds(ILogger logger, uint userId);


        internal struct TimeBounds
        {
            internal readonly Timestamp Start;
            internal readonly Timestamp End;
            internal TimeBounds(Timestamp start, Timestamp end)
            {
                Start = start;
                End = end;
            }
        }
    }
}
