using Microsoft.Extensions.Logging;

using Msz2001.Analytics.Retention.Data;
using Msz2001.MediaWikiDump.HistoryDumpClient.Entities;
using Msz2001.MediaWikiDump.HistoryDumpClient.Reader;

using System.Numerics;

namespace Msz2001.Analytics.Retention.Processors
{
    internal partial class UserEditsProcessor(HistoryDumpReader dumpReader, ILoggerFactory loggerFactory)
    {
        private readonly ILogger logger = loggerFactory.CreateLogger<UserEditsProcessor>();

        public Dictionary<BigInteger, UserData> Process()
        {
            Dictionary<BigInteger, UserData> userDatas = [];

            int i = 0;
            foreach (var entry in dumpReader.ReadEntries())
            {
                i++;
                if (i % 5000 == 0)
                    DisplayProgressMessage(i);

                if (entry.EventUserIsPermanent != true)
                    continue;

                if (entry.EventUserId is null || entry.EventUserTextHistorical is null)
                    continue;

                var userId = (BigInteger)entry.EventUserId;
                var userName = entry.EventUserTextCurrent;

                if (!userDatas.TryGetValue(userId, out var userData))
                {
                    userData = new UserData(
                        userName,
                        entry.EventUserRegistrationTimestamp ?? entry.EventUserCreationTimestamp
                            ?? entry.EventUserFirstEditTimestamp,
                        entry.EventUserFirstEditTimestamp
                    );
                    userDatas[userId] = userData;
                }

                if (entry.EventEntity == "revision")
                {
                    userData.TotalEdits++;

                    var editTime = entry.EventTimestamp;
                    if (editTime > userData.RegistrationDate && editTime < userData.RegistrationPlusDay)
                        userData.Edits_reg1d++;
                    if (editTime > userData.RegistrationPlusMonth && editTime < userData.RegistrationPlus2Months)
                        userData.Edits_regm2++;
                    if (editTime > userData.FirstEditPlusMonth && editTime < userData.FirstEditPlus2Months)
                        userData.Edits_1em2++;
                    if (editTime > userData.FirstEditPlus2Months)
                        userData.Edits_1eplus60++;

                    userData.IsBot |= entry.EventUserIsBotByHistorical.Any(x => x == "group");
                    userData.IsCrossWiki |=
                        entry.EventUserIsCreatedBySystem == true
                        && userData.RegistrationDate < userData.FirstEditDate;
                }

                if (entry is UserHistoryDumpEntry userEntry && entry.EventType == "create")
                {
                    userData.IsBot |= userEntry.UserIsBotByHistorical.Any(x => x == "group");
                    userData.IsCrossWiki |=
                        userEntry.UserIsCreatedBySystem == true
                        && userData.RegistrationDate < userData.FirstEditDate;
                }
            }

            return userDatas;
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
