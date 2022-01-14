using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PQS.MessageBusFun.Messages
{
    public interface DoJob
    {
        public string GroupId { get; }
        public int Index { get; }
        public int Count { get; }
        public string Path { get; }
    }
}
