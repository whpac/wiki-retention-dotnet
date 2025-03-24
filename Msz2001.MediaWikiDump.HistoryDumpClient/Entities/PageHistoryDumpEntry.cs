using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.MediaWikiDump.HistoryDumpClient.Entities
{
    public class PageHistoryDumpEntry : HistoryDumpEntry
    {
        public BigInteger? PageId { get; }
        public string PageTitleHistorical { get; }
        public string PageTitle { get; }
        public int? PageNamespaceHistorical { get; }
        public bool? PageNamespaceIsContentHistorical { get; }
        public int? PageNamespace { get; }
        public bool? PageNamespaceIsContent { get; }
        public bool? PageIsRedirect { get; }
        public bool? PageIsDeleted { get; }
        public DateTime? PageCreationTimestamp { get; }
        public DateTime? PageFirstEditTimestamp { get; }
        public BigInteger? PageRevisionCount { get; }
        public BigInteger? PageSecondsSincePreviousRevision { get; }

        internal PageHistoryDumpEntry(string[] columns) : base(columns)
        {
            PageId = ParseBigIntNullable(columns[25]);
            PageTitleHistorical = columns[26];
            PageTitle = columns[27];
            PageNamespaceHistorical = ParseIntNullable(columns[28]);
            PageNamespaceIsContentHistorical = ParseBoolNullable(columns[29]);
            PageNamespace = ParseIntNullable(columns[30]);
            PageNamespaceIsContent = ParseBoolNullable(columns[31]);
            PageIsRedirect = ParseBoolNullable(columns[32]);
            PageIsDeleted = ParseBoolNullable(columns[33]);
            PageCreationTimestamp = ParseTimestampNullable(columns[34]);
            PageFirstEditTimestamp = ParseTimestampNullable(columns[35]);
            PageRevisionCount = ParseBigIntNullable(columns[36]);
            PageSecondsSincePreviousRevision = ParseBigIntNullable(columns[37]);
        }
    }
}
