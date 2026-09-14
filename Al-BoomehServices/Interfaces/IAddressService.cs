using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices.Interfaces
{
    public interface IAddressService
    {
        Task<AddressDTO> GetAddress(int Id);
        Task<int> AddAddress(AddressDTO addressDTO);
        Task<bool> DeleteAddress(int id);
        Task<bool> UpdateAddress(AddressDTO addressDTO);
        Task<bool> IsExist(int id);
    }
}