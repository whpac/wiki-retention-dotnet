using Msz2001.MediaWikiDump.XmlDumpClient.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Data
{
    internal struct BlockSpan
    {
        public Timestamp Start;
        public Timestamp End;

        public readonly TimeSpan Duration => End - Start;
    }
}
