using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Dtos
{
    public class DevicesDto
    {
        public string InstallationId { get; set; }
       public  Al_BoomehServices.Services.DevicesService.Platform Platform { get; set; }
    }
}
