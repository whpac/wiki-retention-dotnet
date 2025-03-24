using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.MediaWikiDump.HistoryDumpClient.Entities
{
    public class UserHistoryDumpEntry : HistoryDumpEntry
    {
        public BigInteger UserId { get; }
        public string UserTextHistorical { get; }
        public string UserText { get; }
        public string[] UserBlocksHistorical { get; }
        public string[] UserBlocks { get; }
        public string[] UserGroupsHistorical { get; }
        public string[] UserGroups { get; }
        public string[] UserIsBotByHistorical { get; }
        public string[] UserIsBotBy { get; }
        public bool UserIsCreatedBySelf { get; }
        public bool UserIsCreatedBySystem { get; }
        public bool UserIsCreatedByPeer { get; }
        public bool UserIsAnonymous { get; }
        public bool UserIsTemporary { get; }
        public bool UserIsPermanent { get; }
        public DateTime? UserRegistrationTimestamp { get; }
        public DateTime? UserCreationTimestamp { get; }
        public DateTime? UserFirstEditTimestamp { get; }

        internal UserHistoryDumpEntry(string[] columns) : base(columns)
        {
            UserId = ParseBigInt(columns[38]);
            UserTextHistorical = columns[39];
            UserText = columns[40];
            UserBlocksHistorical = ParseArray(columns[41]);
            UserBlocks = ParseArray(columns[42]);
            UserGroupsHistorical = ParseArray(columns[43]);
            UserGroups = ParseArray(columns[44]);
            UserIsBotByHistorical = ParseArray(columns[45]);
            UserIsBotBy = ParseArray(columns[46]);
            UserIsCreatedBySelf = ParseBool(columns[47]);
            UserIsCreatedBySystem = ParseBool(columns[48]);
            UserIsCreatedByPeer = ParseBool(columns[49]);
            UserIsAnonymous = ParseBool(columns[50]);
            UserIsTemporary = ParseBool(columns[51]);
            UserIsPermanent = ParseBool(columns[52]);
            UserRegistrationTimestamp = ParseTimestampNullable(columns[53]);
            UserCreationTimestamp = ParseTimestampNullable(columns[54]);
            UserFirstEditTimestamp = ParseTimestampNullable(columns[55]);
        }
    }
}
