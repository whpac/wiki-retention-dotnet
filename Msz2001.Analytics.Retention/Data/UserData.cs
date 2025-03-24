using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Data
{
    internal class UserData(string userName, DateTime? registrationDate, DateTime? firstEditDate)
    {
        public readonly string UserName = userName;
        public TimeSpan BlockDuration = TimeSpan.Zero;

        public readonly DateTime? RegistrationDate = registrationDate;
        public readonly DateTime? FirstEditDate = firstEditDate;

        public readonly DateTime? RegistrationPlusDay = registrationDate?.AddDays(1);
        public readonly DateTime? RegistrationPlusMonth = registrationDate?.AddMonths(1);
        public readonly DateTime? RegistrationPlus2Months = registrationDate?.AddMonths(2);
        public readonly DateTime? FirstEditPlusMonth = firstEditDate?.AddMonths(1);
        public readonly DateTime? FirstEditPlus2Months = firstEditDate?.AddMonths(2);

        public uint Edits_reg1d = 0;
        public uint Edits_regm2 = 0;
        public uint Edits_1em2 = 0;
        public uint TotalEdits = 0;

        public bool IsBot = false;
        public bool IsCrossWiki = false;

        public DateTime? GetBaselineDate()
        {
            var regDate = RegistrationDate ?? FirstEditDate;
            if (regDate is null)
                return null;

            if (FirstEditDate is null)
                return regDate;

            return regDate < FirstEditDate ? regDate : FirstEditDate;
        }

        public double BlockDays => BlockDuration.TotalDays;
    }
}
