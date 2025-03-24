using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Utils
{
    internal static class RelativeTimeParser
    {
        public static TimeSpan ParseRelativeTime(string timeString)
        {
            Dictionary<string, int> parts = new()
            {
                ["days"] = 0,
                ["hours"] = 0,
                ["minutes"] = 0,
                ["seconds"] = 0
            };
            Dictionary<string, (string, int)> keywords = new()
            {
                ["years"] = ("days", 365), // Not precisely aligned with GNU
                ["months"] = ("days", 30), // Not precisely aligned with GNU
                ["fortnights"] = ("days", 14),
                ["weeks"] = ("days", 7),
                ["days"] = ("days", 1),
                ["hours"] = ("hours", 1),
                ["minutes"] = ("minutes", 1),
                ["mins"] = ("minutes", 1),
                ["seconds"] = ("seconds", 1),
                ["secs"] = ("seconds", 1),

                ["now"] = ("seconds", 0),
                ["today"] = ("days", 0),
                ["yesterday"] = ("days", -1),
                ["tomorrow"] = ("days", 1),
            };

            int currMultiplier = 0;
            string? currUnit = null;

            string[] timeParts = (timeString + " 0").ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in timeParts)
            {
                if (int.TryParse(part, out int num))
                {
                    if (currUnit is not null)
                    {
                        var (unitName, unitValue) = keywords[currUnit];
                        parts[unitName] += num * unitValue;
                    }
                    currMultiplier = num;
                    currUnit = null;
                }
                else if (keywords.ContainsKey(part))
                {
                    currUnit = part;
                }
                else if (keywords.ContainsKey(part + "s"))
                {
                    currUnit = part + "s";
                }
                else if (part == "ago")
                {
                    currMultiplier *= -1;
                }
                else
                {
                    throw new FormatException("Unknown part: " + part);
                }
            }

            return new TimeSpan(
                parts["days"],
                parts["hours"],
                parts["minutes"],
                parts["seconds"]
            );
        }
    }
}
