using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.MediaWikiDump.HistoryDumpClient.Entities
{
    public class RevisionHistoryDumpEntry : PageHistoryDumpEntry
    {
        public BigInteger RevisionId { get; }
        public BigInteger? RevisionParentId { get; }
        public bool RevisionMinorEdit { get; }
        public string[] RevisionDeletedParts { get; }
        public bool RevisionDeletedPartsSuppressed { get; }
        public BigInteger? RevisionTextBytes { get; }
        public BigInteger? RevisionTextBytesDiff { get; }
        public string RevisionTextSha1 { get; }
        public string RevisionContentModel { get; }
        public string RevisionContentFormat { get; }
        public bool RevisionIsDeletedByPageDeletion { get; }
        public DateTime? RevisionDeletedByPageDeletionTimestamp { get; }
        public bool RevisionIsIdentityReverted { get; }
        public BigInteger? RevisionFirstIdentityRevertingRevisionId { get; }
        public BigInteger? RevisionSecondsToIdentityRevert { get; }
        public bool RevisionIsIdentityRevert { get; }
        public bool RevisionIsFromBeforePageCreation { get; }
        public string[] RevisionTags { get; }

        internal RevisionHistoryDumpEntry(string[] columns) : base(columns)
        {
            RevisionId = ParseBigInt(columns[56]);
            RevisionParentId = ParseBigIntNullable(columns[57]);
            RevisionMinorEdit = ParseBool(columns[58]);
            RevisionDeletedParts = ParseArray(columns[59]);
            RevisionDeletedPartsSuppressed = ParseBool(columns[60]);
            RevisionTextBytes = ParseBigIntNullable(columns[61]);
            RevisionTextBytesDiff = ParseBigIntNullable(columns[62]);
            RevisionTextSha1 = columns[63];
            RevisionContentModel = columns[64];
            RevisionContentFormat = columns[65];
            RevisionIsDeletedByPageDeletion = ParseBool(columns[66]);
            RevisionDeletedByPageDeletionTimestamp = ParseTimestampNullable(columns[67]);
            RevisionIsIdentityReverted = ParseBool(columns[68]);
            RevisionFirstIdentityRevertingRevisionId = ParseBigIntNullable(columns[69]);
            RevisionSecondsToIdentityRevert = ParseBigIntNullable(columns[70]);
            RevisionIsIdentityRevert = ParseBool(columns[71]);
            RevisionIsFromBeforePageCreation = ParseBool(columns[72]);
            RevisionTags = ParseArray(columns[73]);
        }
    }
}
