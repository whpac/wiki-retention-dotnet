﻿using Msz2001.MediaWikiDump.XmlDumpClient.Entities;
using Msz2001.MediaWikiDump.XmlDumpClient.Parsers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Msz2001.MediaWikiDump.XmlDumpClient
{
    public class LogDumpReader(XmlReader xmlReader) : DumpReader<LogItem>(xmlReader)
    {

        protected override LogItem? ProcessContentElement(XElement element, SiteInfo siteinfo)
        {
            return LogItemParser.Parse(element, siteinfo);
        }
    }
}
