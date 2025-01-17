using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msz2001.Analytics.Retention.Instrumentation
{
    internal interface IExtractor<in TEntry>
    {
        string Name { get; }

        void BeforeProcessing();

        void ProcessEntry(TEntry entry);

        void FinishProcessing();
    }
}
