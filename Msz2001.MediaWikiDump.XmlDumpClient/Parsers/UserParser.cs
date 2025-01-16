using Msz2001.MediaWikiDump.XmlDumpClient.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Msz2001.MediaWikiDump.XmlDumpClient.Parsers
{
    internal class UserParser
    {
        internal static User Parse(XElement elem)
        {
            uint? id = null;
            string? name = null, ip = null;

            foreach (var child in elem.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "id":
                        id = uint.Parse(child.Value);
                        break;
                    case "username":
                        name = child.Value;
                        break;
                    case "ip":
                        ip = child.Value;
                        break;
                    default:
                        Console.WriteLine($"Unexpected element in user: {child.Name.LocalName}");
                        break;
                }
            }

            if (id is null && name is null && ip is null)
                throw new InvalidOperationException("User element should have either <ip> or <id> and <username> tags.");

            if ((id is null && name is not null) || (id is not null && name is null))
                throw new InvalidOperationException("Both <id> and <username> have to be set if one is present.");

            if ((id is not null || name is not null) && ip is not null)
                throw new InvalidOperationException("<ip> cannot be specified together with <ip> or <id>.");

            return new User
            {
                Id = id,
                Name = name,
                IP = ip,
            };
        }
    }
}
