using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IProcessedMessageService
    {
        Task<bool> TryAdd(Guid messageId, string consumer);
        Task Remove(Guid messageId, string consumer);
    }
}
