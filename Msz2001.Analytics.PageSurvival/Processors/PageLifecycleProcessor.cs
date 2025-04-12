﻿using Microsoft.Extensions.Logging;

using Msz2001.Analytics.PageSurvival.Data;
using Msz2001.MediaWikiDump.XmlDumpClient;
using Msz2001.MediaWikiDump.XmlDumpClient.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.PageSurvival.Processors
{
    internal partial class PageLifecycleProcessor(LogDumpReader logReader, ILoggerFactory loggerFactory)
    {
        private readonly ILogger logger = loggerFactory.CreateLogger<PageLifecycleProcessor>();

        private Dictionary<string, PageEvents> existingPages = [];
        private List<PageEvents> deletedPages = [];

        public IEnumerable<PageEvents> Process()
        {
            int i = 0;
            foreach (var logItem in logReader)
            {
                i++;
                if (i % 5000 == 0)
                    DisplayProgressMessage(i);

                if (logItem.Title is null) continue;

                if (logItem.Type == "create" && logItem.Title.Namespace.Id == 0)
                {
                    RecordPageCreation(logItem.Title.ToString(), logItem.Timestamp);
                }
                else if (logItem.Type == "move")
                {
                    var oldTitle = logItem.Title.ToString();
                    var newTitle = "";
                    if (logItem.Params is string paramsText)
                    {
                        newTitle = paramsText;
                    }
                    else if (logItem.Params is Dictionary<object, object?> paramsDict)
                    {
                        newTitle = paramsDict.GetValueOrDefault("4::target") as string ?? "";
                    }

                    if (string.IsNullOrEmpty(newTitle)) continue;

                    var isOldTitleMainNS = logItem.Title.Namespace.Id == 0;
                    var isNewTitleMainNS = !newTitle.Contains(':');
                    if (logReader.SiteInfo is not null)
                    {
                        var newTitleObject = Title.FromText(newTitle, logReader.SiteInfo);
                        isNewTitleMainNS = newTitleObject.Namespace.Id == 0;
                    }

                    if (isOldTitleMainNS == isNewTitleMainNS)
                    {
                        RecordPageMove(oldTitle, newTitle);
                    }
                    else if (isOldTitleMainNS && !isNewTitleMainNS)
                    {
                        RecordPageDeletion(oldTitle, logItem.Timestamp);
                    }
                    else if (!isOldTitleMainNS && isNewTitleMainNS)
                    {
                        RecordPageCreation(newTitle, logItem.Timestamp);
                    }
                }
                else if (logItem.Type == "delete" && logItem.Action == "delete")
                {
                    RecordPageDeletion(logItem.Title.ToString(), logItem.Timestamp);
                }
            }

            return deletedPages.Concat(existingPages.Values);
        }

        void RecordPageCreation(string title, Timestamp date)
        {
            var page = new PageEvents(date);
            existingPages[title] = page;
        }

        void RecordPageMove(string oldTitle, string newTitle)
        {
            if (!existingPages.TryGetValue(oldTitle, out var page)) return;

            existingPages.Remove(oldTitle);
            existingPages[newTitle] = page;
        }

        void RecordPageDeletion(string title, Timestamp date)
        {
            if (!existingPages.TryGetValue(title, out var page)) return;

            page.Deleted = date;
            deletedPages.Add(page);
            existingPages.Remove(title);
        }

        void DisplayProgressMessage(int iteration)
        {
            var memory = Environment.WorkingSet / (1024.0 * 1024.0);
            LogProcessingProgress(logger, iteration, memory);
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Processed {Iteration} entries, using {Memory:F2} MB")]
        static partial void LogProcessingProgress(ILogger logger, int iteration, double memory);
    }
}
