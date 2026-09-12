using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Classes
{
    public class AddressService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(AppDbContext context,ILogger<AddressService> logger)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<AddressDTO> GetAddress(int Id)
        {
            var address = await _context.Addresses
                .Where(a => a.Id == Id)
                .Select(n => new AddressDTO
                {
                    id=Id,
                    customerId = n.CustomerId,
                    Latitude = n.Latitude,
                    Longitude = n.Longitude,
                    AddressName = n.AddressName,
                    Phone = n.Phone,
                    AdditionalPhone = n.AdditionalPhone,
                    HomeNum = n.HomeNum,
                    BuildNum = n.BuildNum,
                    FloorNum = n.FloorNum,
                    StreetName = n.StreetName,
                }).AsNoTracking().FirstOrDefaultAsync();
            if (address == null) return null;
            return address;
        }
        public async Task<int> AddAddress(AddressDTO addressDTO)
        {
            if (addressDTO == null) return -1;
            Models.Address newaddress = new Models.Address 
            {
                CustomerId = addressDTO.customerId,
                Latitude = addressDTO.Latitude,
                Longitude = addressDTO.Longitude,
                Notes = addressDTO.Notes,
                Phone = addressDTO.Phone,
                HomeNum = addressDTO.HomeNum,
                BuildNum = addressDTO.BuildNum,
                StreetName = addressDTO.StreetName,
                FloorNum = addressDTO.FloorNum,
                AddressName = addressDTO.AddressName,
                AdditionalPhone = addressDTO.AdditionalPhone,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _context.Addresses.AddAsync(newaddress);
            await _context.SaveChangesAsync();
            return newaddress.Id;

        }
        public async Task<bool> DeleteAddress(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            address.IsDeleted = true;
            address.DeletedAtUtc = DateTime.UtcNow;
            
            int roweffected = await _context.SaveChangesAsync();

            return (roweffected > 0);
        }
        public async Task<bool> UpdateAddress(AddressDTO addressDTO)
        {
            var address = await _context.Addresses.FindAsync(addressDTO.id);
            address.StreetName = addressDTO.StreetName;
            address.Latitude = addressDTO.Latitude;
            address.Longitude = addressDTO.Longitude;
            address.Notes = addressDTO.Notes;
            address.Phone = addressDTO.Phone;
            address.HomeNum = addressDTO.HomeNum;
            address.AddressName = addressDTO.AddressName;
            address.AdditionalPhone = addressDTO.AdditionalPhone;
            address.FloorNum = addressDTO.FloorNum;
            address.BuildNum = addressDTO.BuildNum;
            address.Phone = addressDTO.Phone;
            address.UpdatedAtUtc = DateTime.UtcNow;
            int rowseffefcted = await _context.SaveChangesAsync();
            if ((rowseffefcted > 0))
            {
                _logger.LogInformation("successfully updated address for customerId {CustomerId}",
              addressDTO.customerId);
                return true;
            }
            _logger.LogWarning("Update Address failed with id {AddressId}",
              addressDTO.id);
            throw new BusinessRuleException($"Update Address failed");
        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Addresses
                .AnyAsync(a => a.Id == id);
            return result;
        }
    }
}
