using Msz2001.Analytics.Retention.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Classifiers
{
    internal interface IClassifier
    {
        string[] Classes { get; }

        string Classify(UserData user);
    }
}
