using Msz2001.Analytics.Retention.Data;

using System.Globalization;
using System.Numerics;

namespace Msz2001.Analytics.Retention.Writers
{
    internal static class UserDataWriter
    {
        public static void Write(string fileName, Dictionary<BigInteger, UserData> data)
        {

            using var fileStream = File.OpenWrite(fileName);
            using var writer = new StreamWriter(fileStream);

            writer.Write("UserId\tUserName\tRegistrationDate\tFirstEditDate\t");
            writer.Write("TotalEdits\tEdits_reg1d\tEdits_regm2\tEdits_1em2\t");
            writer.Write("IsBot\tIsCrossWiki\tBlockDays\n");

            foreach (var (userId, row) in data)
            {
                writer.WriteLine(
                    string.Join('\t', [
                        userId,
                        row.UserName,
                        TimestampToString(row.RegistrationDate),
                        TimestampToString(row.FirstEditDate),
                        row.TotalEdits,
                        row.Edits_reg1d,
                        row.Edits_regm2,
                        row.Edits_1em2,
                        row.IsBot,
                        row.IsCrossWiki,
                        row.BlockDays.ToString("F3")
                    ])
                );
            }
        }

        private static string TimestampToString(DateTime? timestamp)
        {
            return timestamp?.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture) ?? "";
        }
    }
}
