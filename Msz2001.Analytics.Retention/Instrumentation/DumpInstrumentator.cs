using Microsoft.Extensions.Logging;

using Msz2001.MediaWikiDump.XmlDumpClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Instrumentation
{
    internal partial class DumpInstrumentator(ILogger logger)
    {
        private readonly ILogger Logger = logger;
        internal int ReportingInterval = 5000;

        internal void ProcessDump<TEntry>(DumpReader<TEntry> dumpReader, IEnumerable<IExtractor<TEntry>> extractors) where TEntry : IDumpEntry
        {
            LogStartupMessage(Logger, dumpReader.GetType().Name);

            List<IExtractor<TEntry>> initializedExtractors = [];

            foreach (var extractor in extractors)
            {
                try
                {
                    LogBeforeProcessing(Logger, extractor.Name);
                    extractor.BeforeProcessing();
                    initializedExtractors.Add(extractor);
                }
                catch (Exception ex)
                {
                    LogExceptionExtractorBeforeProcessing(Logger, extractor.Name, ex);
                }
            }

            if (initializedExtractors.Count == 0)
            {
                LogNoExtractors(Logger);
                return;
            }

            int i = 0;
            foreach (var entry in dumpReader)
            {
                foreach (var extractor in initializedExtractors)
                {
                    try
                    {
                        extractor.ProcessEntry(entry);
                    }
                    catch (Exception ex)
                    {
                        LogExceptionWhileProcessing(Logger, entry.Id, extractor.Name, ex);
                    }
                }

                if (++i % ReportingInterval == 0)
                    DisplayProgressMessage(i);
            }
            DisplayProgressMessage(i);

            foreach (var extractor in initializedExtractors)
            {
                LogFinishProcessing(Logger, extractor.Name);
                extractor.FinishProcessing();
            }

            LogFinishMessage(Logger, dumpReader.GetType().Name);
        }

        void DisplayProgressMessage(int iteration)
        {
            var memory = Environment.WorkingSet / (1024.0 * 1024.0);
            LogProcessingProgress(Logger, iteration, memory);
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Started dump instrumentation {ReaderName}")]
        static partial void LogStartupMessage(ILogger logger, string readerName);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Calling BeforeProcessing on {ExtractorName}")]
        static partial void LogBeforeProcessing(ILogger logger, string extractorName);

        [LoggerMessage(Level = LogLevel.Information, Message = "Processed {Iteration} entries, using {Memory:F2} MB")]
        static partial void LogProcessingProgress(ILogger logger, int iteration, double memory);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Calling FinishProcessing on {ExtractorName}")]
        static partial void LogFinishProcessing(ILogger logger, string extractorName);

        [LoggerMessage(Level = LogLevel.Information, Message = "Finished dump instrumentation {ReaderName}")]
        static partial void LogFinishMessage(ILogger logger, string readerName);

        [LoggerMessage(Level = LogLevel.Warning, Message = "No extractors were initialized. Aborting.")]
        static partial void LogNoExtractors(ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error in BeforeProcessing on {ExtractorName}. The extractor will be skipped.")]
        static partial void LogExceptionExtractorBeforeProcessing(ILogger logger, string extractorName, Exception ex);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error while processing an entry #{EntryId} by {ExtractorName}")]
        static partial void LogExceptionWhileProcessing(ILogger logger, uint entryId, string extractorName, Exception ex);
    }
}
