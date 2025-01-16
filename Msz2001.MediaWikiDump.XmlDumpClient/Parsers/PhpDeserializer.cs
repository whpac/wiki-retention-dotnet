using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.MediaWikiDump.XmlDumpClient.Parsers
{
    internal static class PhpDeserializer
    {

        internal static object? Deserialize(string input)
        {
            int position = 0;
            return DeserializeInner(input.AsSpan(), ref position);
        }

        internal static bool TryDeserialize(string input, out object? result)
        {
            try
            {
                result = Deserialize(input);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static object? DeserializeInner(ReadOnlySpan<char> input, ref int position)
        {
            char type = input[position];

            // For nulls, it's the place where the next value starts,
            // For others it's the place where the value or metadata start.
            position += 2;

            if (type == 'N')
                return null;

            int valueStart = position;
            char c;
            do
            {
                c = input[position];
                position++;
            } while (c != ';' && c != ':');

            var value = input[valueStart..(position - 1)];

            switch (type)
            {
                case 'N':
                    return null;
                case 'b':
                    return value[0] == '1';
                case 'i':
                    return int.Parse(value);
                case 'd':
                    return value switch
                    {
                        "NAN" => double.NaN,
                        "INF" => double.PositiveInfinity,
                        "-INF" => double.NegativeInfinity,
                        _ => (object)double.Parse(value),
                    };
                case 's':
                    // value is the length of the string
                    position++; // skip the opening quote
                    var str = input[position..(position+int.Parse(value))];
                    position += int.Parse(value) + 2; // skip the closing quote and the semicolon
                    return str.ToString();
                case 'a':
                    // value is the length of the array
                    Dictionary<object, object?> dict = [];
                    position++; // skip the opening brace
                    for (int i = 0; i < int.Parse(value); i++)
                    {
                        object? key = DeserializeInner(input, ref position);
                        object? val = DeserializeInner(input, ref position);

                        if (key is not int and not string)
                            throw new NotSupportedException("Only strings and integers can be used as array keys.");

                        dict[key] = val;
                    }
                    position++; // skip the closing brace
                    return dict;
                case 'O':
                    throw new NotSupportedException("Deserializing of PHP objects is not supported.");
                default:
                    throw new NotSupportedException($"Unknown type: {type}");
            }
        }
    }
}
