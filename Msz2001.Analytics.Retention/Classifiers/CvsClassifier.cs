using Msz2001.Analytics.Retention.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Classifiers
{
    internal class CvsClassifier : IClassifier
    {
        public string[] Classes => ["Survived", "NotSurvived", "NoEdits"];


        public string Classify(UserData user)
        {
            return user switch
            {
                { Edits_1eplus60: > 0 } => "Survived",
                { TotalEdits: > 0 } => "NotSurvived",
                _ => "NoEdits"
            };
        }
    }
}
