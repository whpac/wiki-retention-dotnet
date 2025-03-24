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
    internal partial class FirstEdits(StreamWriter OutputStream, ILogger logger) : IExtractor<Page>
    {
        public string Name => nameof(FirstEdits);
        private readonly ILogger Logger = logger;
        private readonly Dictionary<uint, UserEdit> FirstUserEdits = new();

        public void BeforeProcessing() { }

        public void ProcessEntry(Page entry)
        {
            foreach (var rev in entry.Revisions)
            {
                if (rev.User is null || rev.User.Id is null) continue;

                if (!FirstUserEdits.TryGetValue(rev.User.Id.Value, out UserEdit storedValue))
                {
                    FirstUserEdits[rev.User.Id.Value] = new UserEdit(rev.Timestamp, rev.Id);
                }
                else if (rev.Timestamp < storedValue.Timestamp)
                {
                    FirstUserEdits[rev.User.Id.Value] = new UserEdit(rev.Timestamp, rev.Id);
                }
            }
        }

        public void FinishProcessing()
        {
            WriteOutputRecord("user_id", "timestamp", "rev_id");

            foreach (var rev in FirstUserEdits)
            {
                WriteOutputRecord(rev.Key, rev.Value.Timestamp.ToString(), rev.Value.RevisionId);
            }

            OutputStream.Flush();
        }

        protected void WriteOutputRecord(params object[] columns)
        {
            OutputStream.WriteLine(string.Join("\t", columns));
        }


        private struct UserEdit
        {
            internal readonly Timestamp Timestamp;
            internal readonly uint RevisionId;

            internal UserEdit(Timestamp timestamp, uint revisionId)
            {
                Timestamp = timestamp;
                RevisionId = revisionId;
            }
        }
    }
}
