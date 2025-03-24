using Microsoft.Extensions.Logging;

using Msz2001.Analytics.Retention.Data;
using Msz2001.Analytics.Retention.Utils;
using Msz2001.MediaWikiDump.XmlDumpClient;
using Msz2001.MediaWikiDump.XmlDumpClient.Entities;

using System.Text.RegularExpressions;

namespace Msz2001.Analytics.Retention.Processors
{
    internal partial class BlockedUsersProcessor(LogDumpReader logReader, ILoggerFactory loggerFactory)
    {
        private readonly ILogger logger = loggerFactory.CreateLogger<BlockedUsersProcessor>();

        public Dictionary<string, List<BlockSpan>> Process()
        {
            Dictionary<string, List<BlockSpan>> userBlocks = [];

            int i = 0;
            foreach (var logItem in logReader)
            {
                i++;
                if (i % 5000 == 0)
                    DisplayProgressMessage(i);

                if (logItem.Type != "block")
                    continue;

                var blockedUser = logItem.Title;
                if (blockedUser is null)
                    continue;

                var blockedUserName = blockedUser.PageName;
                if (IsUserAnonymous(blockedUserName))
                    continue;

                if (!userBlocks.TryGetValue(blockedUserName, out var blocks))
                {
                    blocks = [];
                    userBlocks[blockedUserName] = blocks;
                }

                try
                {
                    switch (logItem.Action)
                    {
                        case "block":
                            UpdateBlockListBlock(blocks, logItem);
                            break;
                        case "unblock":
                            UpdateBlockListUnblock(blocks, logItem);
                            break;
                        case "reblock":
                            UpdateBlockListReblock(blocks, logItem);
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogError(logger, e.Message);
                }
            }

            return userBlocks;
        }

        private static bool IsUserAnonymous(string userName)
        {
            return userName.StartsWith('#') // Likely autoblocks that were unblocked
                || userName.StartsWith("~20") // Temp accounts
                || IPv4Regex().IsMatch(userName)
                || IPv6Regex().IsMatch(userName);
        }

        private static void UpdateBlockListBlock(List<BlockSpan> blocks, LogItem blockEntry)
        {
            var newBlock = new BlockSpan
            {
                Start = blockEntry.Timestamp,
                End = CalculateBlockEndDate(blockEntry)
            };
            blocks.Add(newBlock);
        }

        private static void UpdateBlockListReblock(List<BlockSpan> blocks, LogItem blockEntry)
        {
            if (blocks.Count == 0)
                return;

            var lastBlock = blocks[^1];
            lastBlock.End = CalculateBlockEndDate(blockEntry);
            blocks[^1] = lastBlock;
        }

        private static void UpdateBlockListUnblock(List<BlockSpan> blocks, LogItem blockEntry)
        {
            if (blocks.Count == 0)
                return;

            var lastBlock = blocks[^1];
            lastBlock.End = blockEntry.Timestamp;
            blocks[^1] = lastBlock;
        }

        private static Timestamp CalculateBlockEndDate(LogItem entry)
        {
            string blockDuration = "";
            Timestamp blockEndDate;
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

            if (blockDuration == "") {
                throw new Exception("No block duration found");
            }
            else if (Timestamp.TryParse(blockDuration, out blockEndDate)) { }
            else
            {
                blockEndDate = entry.Timestamp + RelativeTimeParser.ParseRelativeTime(blockDuration);
            }

            return blockEndDate;
        }

        void DisplayProgressMessage(int iteration)
        {
            var memory = Environment.WorkingSet / (1024.0 * 1024.0);
            LogProcessingProgress(logger, iteration, memory);
        }

        // Is seems that MediaWiki uses a similar regex for IPv4
        // so that 256.0.0.0 is an invalid username, even though it's
        // not a valid IP
        [GeneratedRegex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(/\d+)?$")]
        private static partial Regex IPv4Regex();

        // This catches a broader class of strings than just valid IPv6
        // but colon is forbidden in usernames since 2015, so that
        // this shouldn't generate too many false positives
        [GeneratedRegex(@"^[0-9a-fA-F:]+(/\d+)?$")]
        private static partial Regex IPv6Regex();

        [LoggerMessage(Level = LogLevel.Information, Message = "Processed {Iteration} entries, using {Memory:F2} MB")]
        static partial void LogProcessingProgress(ILogger logger, int iteration, double memory);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error while processing block entry: {Error}")]
        static partial void LogError(ILogger logger, string error);
    }
}
