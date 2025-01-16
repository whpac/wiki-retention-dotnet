using Msz2001.MediaWikiDump.XmlDumpClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Instrumentation
{
    internal class DumpInstrumentator
    {
        internal int ReportingInterval = 1000;

        internal void ProcessDump<TEntry>(DumpReader<TEntry> dumpReader, IEnumerable<IExtractor<TEntry>> extractors)
        {
            foreach (var extractor in extractors)
            {
                extractor.BeforeProcessing();
            }

            int i = 0;
            foreach (var entry in dumpReader)
            {
                foreach (var extractor in extractors)
                {
                    extractor.ProcessEntry(entry);
                }

                if (++i % ReportingInterval == 0)
                    DisplayProgressMessage(i);
            }
            DisplayProgressMessage(i);

            foreach (var extractor in extractors)
            {
                extractor.FinishProcessing();
            }
        }

        void DisplayProgressMessage(int iteration)
        {
            var memory = Environment.WorkingSet / (1024.0 * 1024.0);
            Console.WriteLine($"Processed {iteration} entries, using {memory:F2} MB");
        }
    }
}
