using Al_BoomehDAL.Classes;
using Al_BoomehServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IOtpService
    {
        Task Request(string phone);
        Task<OtpService.OtpVerifyResult> Verify(string phone, string code);
    }
}