using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.MediaWikiDump.XmlDumpClient.Entities
{
    public readonly struct Timestamp
    {
        private readonly DateTime Value;

        public Timestamp(DateTime value)
        {
            Value = value.ToUniversalTime();
        }

        public static Timestamp Parse(string value)
        {
            return new Timestamp(DateTime.Parse(value, CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return Value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture);
        }

        public string ToShortString()
        {
            return Value.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
    }
}
