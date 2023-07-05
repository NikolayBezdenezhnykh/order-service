using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class ProcessedMessage
    {
        public Guid Id { get; set; }

        public string Scope { get ; set; }

        public string Result { get; set; }

        public DateTime? TimeStamp { get; set; }
    }
}
