using Al_BoomehServices.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IOrderStatusChanged
    {
        Task Publish(MessageDto message);
    }
}
