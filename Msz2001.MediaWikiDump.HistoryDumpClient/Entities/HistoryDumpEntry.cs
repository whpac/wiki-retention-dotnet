using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.MediaWikiDump.HistoryDumpClient.Entities
{
    public class HistoryDumpEntry
    {
        // Event global fields
        public string WikiDB { get; }
        public string EventEntity { get; }
        public string EventType { get; }
        public DateTime EventTimestamp { get; }
        public string EventComment { get; }

        // Event user fields
        public BigInteger? EventUserId { get; }
        public string EventUserTextHistorical { get; }
        public string EventUserTextCurrent { get; }
        public string[] EventUserBlocksHistorical { get; }
        public string[] EventUserBlocks { get; }
        public string[] EventUserGroupsHistorical { get; }
        public string[] EventUserGroups { get; }
        public string[] EventUserIsBotByHistorical { get; }
        public string[] EventUserIsBotBy { get; }
        public bool? EventUserIsCreatedBySelf { get; }
        public bool? EventUserIsCreatedBySystem { get; }
        public bool? EventUserIsCreatedByPeer { get; }
        public bool? EventUserIsAnonymous { get; }
        public bool? EventUserIsTemporary { get; }
        public bool? EventUserIsPermanent { get; }
        public DateTime? EventUserRegistrationTimestamp { get; }
        public DateTime? EventUserCreationTimestamp { get; }
        public DateTime? EventUserFirstEditTimestamp { get; }
        public BigInteger? EventUserRevisionCount { get; }
        public BigInteger? EventUserSecondsSincePreviousRevision { get; }

        internal HistoryDumpEntry(string[] columns)
        {
            WikiDB = columns[0];
            EventEntity = columns[1];
            EventType = columns[2];
            EventTimestamp = ParseTimestamp(columns[3]);
            EventComment = columns[4];
            EventUserId = ParseBigIntNullable(columns[5]);
            EventUserTextHistorical = columns[6];
            EventUserTextCurrent = columns[7];
            EventUserBlocksHistorical = ParseArray(columns[8]);
            EventUserBlocks = ParseArray(columns[9]);
            EventUserGroupsHistorical = ParseArray(columns[10]); // at the time of edit/skips logs from meta
            EventUserGroups = ParseArray(columns[11]); // now
            EventUserIsBotByHistorical = ParseArray(columns[12]);
            EventUserIsBotBy = ParseArray(columns[13]);
            EventUserIsCreatedBySelf = ParseBoolNullable(columns[14]);
            EventUserIsCreatedBySystem = ParseBoolNullable(columns[15]);
            EventUserIsCreatedByPeer = ParseBoolNullable(columns[16]);
            EventUserIsAnonymous = ParseBoolNullable(columns[17]);
            EventUserIsTemporary = ParseBoolNullable(columns[18]);
            EventUserIsPermanent = ParseBoolNullable(columns[19]);
            EventUserRegistrationTimestamp = ParseTimestampNullable(columns[20]);
            EventUserCreationTimestamp = ParseTimestampNullable(columns[21]);
            EventUserFirstEditTimestamp = ParseTimestampNullable(columns[22]);
            EventUserRevisionCount = ParseBigIntNullable(columns[23]);
            EventUserSecondsSincePreviousRevision = ParseBigIntNullable(columns[24]);
        }

        protected bool? ParseBoolNullable(string raw)
        {
            if (bool.TryParse(raw, out bool result))
            {
                return result;
            }
            return null;
        }

        protected bool ParseBool(string raw)
        {
            return bool.Parse(raw);
        }

        protected int? ParseIntNullable(string raw)
        {
            if (int.TryParse(raw, out int result))
            {
                return result;
            }
            return null;
        }

        protected BigInteger? ParseBigIntNullable(string raw)
        {
            if (BigInteger.TryParse(raw, out BigInteger result))
            {
                return result;
            }
            return null;
        }

        protected BigInteger ParseBigInt(string raw)
        {
            return BigInteger.Parse(raw);
        }

        protected DateTime? ParseTimestampNullable(string raw)
        {
            if (DateTime.TryParseExact(raw, "yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out DateTime result))
            {
                return result;
            }
            return null;
        }

        protected DateTime ParseTimestamp(string raw)
        {
            return DateTime.ParseExact(raw, "yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);
        }

        protected string[] ParseArray(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return [];
            }
            return raw.Split(',');
        }
    }
}
