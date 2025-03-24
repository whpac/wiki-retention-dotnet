using Msz2001.Analytics.Retention.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Classifiers
{
    internal class MszClassifier : IClassifier
    {
        public string[] Classes => [ "Survived", "NotSurvived", "NoEdits", "CrossWiki", "Vandal", "Bot" ];

        public string Classify(UserData user)
        {
            return user switch
            {
                { IsBot: true } => "Bot",
                { BlockDays: > 1 } => "Vandal",
                { IsCrossWiki: true } => "CrossWiki",
                { TotalEdits: 0 } => "NoEdits",
                { Edits_1em2: 0 } => "NotSurvived",
                _ => "Survived"
            };
        }
    }
}
