using Msz2001.Analytics.Retention.Instrumentation;
using Msz2001.MediaWikiDump.XmlDumpClient.Entities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Extractors
{
    internal class RegistrationDates(StreamWriter OutputStream) : IExtractor<LogItem>
    {

        public void BeforeProcessing()
        {
            WriteOutputRecord("user_id", "registration_date", "log_id", "action");
        }

        public void ProcessEntry(LogItem entry)
        {
            if (entry.Type != "newusers")
                return;

            if (entry.User is null || entry.User.Id is null)
                return;

            uint? createdUserId = null;
            if (entry.Params is null || entry.Params as string == "")
            {
                createdUserId = entry.User.Id;
            }
            else if (entry.Params is string paramsStr && uint.TryParse(paramsStr, out uint id))
            {
                createdUserId = id;
            }
            else if (entry.Params is Dictionary<object, object?> paramsDict)
            {
                createdUserId = (uint?)(paramsDict.GetValueOrDefault("4::userid") as int?);
            }

            if (createdUserId is null)
            {
                Console.WriteLine($"Could not extract user ID from {entry.Params}");
                return;
            }

            WriteOutputRecord(
                createdUserId.Value,
                entry.Timestamp.ToString(),
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
    }
}
