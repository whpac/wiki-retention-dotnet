using Microsoft.Extensions.Logging;

using Msz2001.Analytics.PageSurvival.Processors;
using Msz2001.MediaWikiDump.XmlDumpClient.Toolforge;

namespace Msz2001.Analytics.PageSurvival
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
                Console.WriteLine("  outputFile - where to save the result file");
                Console.WriteLine();
                return 1;
            }

            var wikiDB = args[0];
            var fileName = args[1];

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();

            var logReaderFactory = new LogDumpReaderFactory(loggerFactory);
            var logReader = logReaderFactory.CreateReader(wikiDB);
            var pageLifecycleProcessor = new PageLifecycleProcessor(logReader, loggerFactory);
            var pages = pageLifecycleProcessor.Process();

            using var outputFileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            using var outputWriter = new StreamWriter(outputFileStream);

            outputWriter.WriteLine("creation\tdeletion");
            foreach (var page in pages)
            {
                if (page.Deleted is null)
                {
                    outputWriter.WriteLine($"{page.Created}\t-");
                }
                else
                {
                    outputWriter.WriteLine($"{page.Created}\t{page.Deleted}");
                }
            }

            return 0;
        }
    }
}
