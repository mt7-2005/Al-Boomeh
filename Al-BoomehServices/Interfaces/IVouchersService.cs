using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IVouchersService
    {
        Task<int> AddVoucherToCustomer(VoucherDTO voucherDTO);
        Task<bool> IsExist(string code);
        Task<bool> UseVoucher(string code);
        Task<VoucherDTO?> GetVoucherByCode(string code);
    }
}