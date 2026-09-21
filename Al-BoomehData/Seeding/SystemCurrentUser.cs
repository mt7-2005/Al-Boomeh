using Al_BoomehDAL.Interfaces;
using Al_BoomehDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Seeding
{
   public class SystemCurrentUser : ICurrentUser
    {
        public Guid? UserId => null;
        public User.UserRole Role => User.UserRole.None;
        public int? StoreId => null;
        public bool IsAuthenticated => false;
    }

}

