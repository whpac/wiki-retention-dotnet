using Msz2001.Analytics.Retention.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Classifiers
{
    internal class WmfClassifier : IClassifier
    {
        public string[] Classes => ["Survived", "NotSurvived", "NotActivated", "NoEdits"];


        public string Classify(UserData user)
        {
            return user switch
            {
                { Edits_reg1d: > 0, Edits_regm2: > 0 } => "Survived",
                { Edits_reg1d: > 0 } => "NotSurvived",
                { TotalEdits: > 0 } => "NotActivated",
                _ => "NoEdits"
            };
        }
    }
}
