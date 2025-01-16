using Msz2001.MediaWikiDump.XmlDumpClient.Entities;
using Msz2001.MediaWikiDump.XmlDumpClient.Parsers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Msz2001.MediaWikiDump.XmlDumpClient
{
    public class PageDumpReader(XmlReader xmlReader) : DumpReader<Page>(xmlReader)
    {

        protected override Page? ProcessContentElement(XElement element, SiteInfo siteinfo)
        {
            return PageParser.Parse(element, siteinfo);
        }
    }
}
