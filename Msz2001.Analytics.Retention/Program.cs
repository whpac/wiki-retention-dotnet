﻿using Msz2001.Analytics.Retention.Extractors;
using Msz2001.Analytics.Retention.Instrumentation;
using Msz2001.MediaWikiDump.XmlDumpClient;

using System.IO.Compression;
using System.Xml;

namespace Msz2001.Analytics.Retention
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // var logPath = @"D:\Dokumenty\Wikipedia\retention\dumps\plwikinews-20241201-pages-logging.xml.gz";
            // var pagesPath = @"D:\Dokumenty\Wikipedia\retention\dumps\plwikinews-20241201-stub-meta-history.xml.gz";

            // var outputPath = @"D:\regdates.tsv";

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Msz2001.Analytics.Retention <logPath> <outputPath>");
                return;
            }
            var logPath = args[0];
            var outputPath = args[1];

            using var fileStream = new FileStream(logPath, FileMode.Open);
            using var stream = new GZipStream(fileStream, CompressionMode.Decompress);
            var xmlReader = XmlReader.Create(stream);
            var logReader = new LogDumpReader(xmlReader);

            using var streamWriter = new StreamWriter(outputPath);
            var registrationDates = new RegistrationDates(streamWriter);

            var instrumentator = new DumpInstrumentator();

            var start = DateTime.Now;
            instrumentator.ProcessDump(logReader, [registrationDates]);
            var end = DateTime.Now;

            var duration = end - start;
            Console.WriteLine($"Duration: {duration}");
        }
    }
}