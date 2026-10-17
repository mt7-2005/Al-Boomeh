using Al_BoomehDAL.Classes;
using Al_BoomehServices.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IOrderPlaced
    {
        Task Publish(MessageDto message);
    }
}
