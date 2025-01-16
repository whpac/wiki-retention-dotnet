using Msz2001.MediaWikiDump.XmlDumpClient.Entities;
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
    public abstract class DumpReader<TContent>
    {
        private readonly XmlReader XmlReader;
        protected SiteInfo? SiteInfo { get; private set; }

        public DumpReader(XmlReader xmlReader) {
            this.XmlReader = xmlReader;
        }

        public IEnumerable<TContent> GetItems()
        {
            Queue<XElement> contentElems = new();
            SiteInfo? siteinfo = null;
            XElement? element;

            while (XmlReader.Read())
            {
                if (XmlReader.NodeType != XmlNodeType.Element)
                    continue;

                switch (XmlReader.LocalName)
                {
                    case "mediawiki":
                        continue; // Do nothing, this tag is expected. We want to get inside it

                    case "siteinfo":
                        element = XNode.ReadFrom(XmlReader) as XElement ??
                            throw new Exception("Failed to read siteinfo element: it's not an XML element");

                        siteinfo = SiteInfoParser.Parse(element);
                        break;

                    default:
                        element = XNode.ReadFrom(XmlReader) as XElement;
                        if (element is null)
                            continue;

                        if (siteinfo is not null)
                        {
                            TContent? item;
                            try
                            {
                                item = ProcessContentElement(element, siteinfo);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to process element {element.Name.LocalName}: {ex.Message}");
                                continue;
                            }

                            if (item is not null)
                                yield return item;
                        }
                        else
                        {
                            contentElems.Enqueue(element);
                        }
                        break;
                }


                // If we have any XML elems that were held, it's time to process them
                if (siteinfo is not null && contentElems.Count > 0)
                {
                    foreach (var elem in contentElems)
                    {
                        TContent? item;
                        try
                        {
                            item = ProcessContentElement(element, siteinfo);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to process element {element.Name.LocalName}: {ex.Message}");
                            continue;
                        }

                        if (item is not null)
                            yield return item;
                    }
                }
            }
        }

        public IEnumerator<TContent> GetEnumerator()
        {
            return GetItems().GetEnumerator();
        }

        protected abstract TContent? ProcessContentElement(XElement element, SiteInfo siteinfo);
    }
}
