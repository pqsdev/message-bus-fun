using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PQS.MessageBusFun.Messages
{
    public interface JobDone
    {
        string GroupId { get; }
        int Index { get; }
        int Count { get; }
        
    }
}
