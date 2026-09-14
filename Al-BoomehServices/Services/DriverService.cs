using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;


namespace Al_BoomehDAL.Classes
{
   
    public enum enVehicleType
    {
        Car,
        Bicycle
    }
    public class DriversService: IDriversService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DriversService> _logger;
        public DriversService(AppDbContext context,ILogger<DriversService> logger)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<List<DriverInfoDTO>> GetAllDrivers(int pagenumber, int pagesize)
        {
            var driverList = await _context.Drivers
                                    .Select(d => new DriverInfoDTO
                                    {
                                        Id = d.Id,
                                        FirstName = d.FirstName,
                                        LastName = d.LastName,
                                        Phone = d.Phone,
                                        Gender = (enGender)d.Gender,
                                        VehicleType = (enVehicleType)d.VehicleType,
                                        CreatedAt = d.CreatedAtUtc
                                    }).OrderBy(d => d.FirstName)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();

            return driverList;
        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Drivers
                .AnyAsync(a => a.Id == id);
            return result;
        }
        public async Task<List<DriverInfoDTO>> GetDriversByVehicleType(enVehicleType VehicleType, int pagenumber, int pagesize)
        {
            var driverList = await _context.Drivers.Where(d => d.VehicleType == (int)VehicleType)
                                    .Select(d => new DriverInfoDTO
                                    {
                                        Id = d.Id,
                                        FirstName = d.FirstName,
                                        LastName = d.LastName,
                                        Phone = d.Phone,
                                        Gender = (enGender)d.Gender,
                                        VehicleType = (enVehicleType)d.VehicleType,
                                        CreatedAt = d.CreatedAtUtc
                                    }).OrderBy(d => d.FirstName)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();
            return driverList;
        }
        public async Task<DriverInfoDTO> GetDriverById(int driverId)
        {
            var driver = await _context.Drivers
                               .Where(d => d.Id == driverId)
                               .Select(d => new DriverInfoDTO
                               {
                                   Id = d.Id,
                                   FirstName = d.FirstName,
                                   LastName = d.LastName,
                                   Phone = d.Phone,
                                   Gender = (enGender)d.Gender,
                                   VehicleType = (enVehicleType)d.VehicleType,
                                   CreatedAt = d.CreatedAtUtc
                               }).AsNoTracking().FirstOrDefaultAsync();
            if (driver == null) return null;

            return driver;
        }
        public async Task<DriverInfoDTO> GetDriverByPhone(string phone)
        {
            var driver = await _context.Drivers.Where(d => d.Phone == phone)
                .Select(d => new DriverInfoDTO
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Phone = d.Phone,
                    Gender = (enGender)d.Gender,
                    VehicleType = (enVehicleType)d.VehicleType,
                    CreatedAt = d.CreatedAtUtc
                }).AsNoTracking().FirstOrDefaultAsync();
            if (driver == null) return null;
            return driver;
        }
        public async Task<int> CreateDriver(CreateDriverDTO driverDTO)
        {
            if (driverDTO == null) return -1;

            Driver driver = new Driver
            {
                FirstName = driverDTO.FirstName,
                LastName = driverDTO.LastName,
                Phone = driverDTO.Phone,
                Gender = (int)driverDTO.Gender,
                VehicleType = (int)driverDTO.VehicleType,
            };
            _context.Add(driver);
            await _context.SaveChangesAsync();
            return driver.Id;
        }
        public async Task<bool> UpdateDriverInfo(int driverId, UpdateDriverInfoDTO driverDTO)
        {
            var driver = _context.Drivers.Where(d => d.Id == driverId).FirstOrDefault();
            if (driver == null) throw new NotFoundException($"Not found driver with id {driverId}");



            driver.FirstName = driverDTO.FirstName;
            driver.LastName = driverDTO.LastName;
            driver.Phone = driverDTO.Phone;
            driver.VehicleType = (int)driverDTO.VehicleType;
            int rowseffect = await _context.SaveChangesAsync();
            if (rowseffect > 0)
            {
                _logger.LogInformation("update done for driverId {DriverID}",
               driverId);
            }
            _logger.LogWarning("failed to update driver info for ");
            throw new BusinessRuleException($"failed to update driver info");

        }
        public async Task<bool> DeleteDriver(int id)
        {
            var driver = await _context.Drivers
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();
            if (driver == null) throw new NotFoundException($"Not found driver with id {id}");
            _context.Remove(driver);
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<Dictionary<enVehicleType, int>> GetDriverCountPairsVehicleType()
        {
            Dictionary<enVehicleType, int> countpairsvehicletype = new Dictionary<enVehicleType, int>();
            var report = await _context.Drivers
                .GroupBy(d => d.VehicleType)
                .Select(n => new
                {
                    VehicleType = n.Key,
                    Count = n.Count()
                }).OrderBy(o => o.Count).AsNoTracking().ToListAsync();
            foreach (var row in report)
            {
                countpairsvehicletype.Add((enVehicleType)row.VehicleType, row.Count);
            }
            return countpairsvehicletype;
        }
        public async Task<bool> IsDriverExistByPhone(string phone)
        {
            var exist = await _context.Drivers
                .AnyAsync(d => d.Phone == phone);
            return exist;
        }
        public async Task<List<DriverIssueDTO>> GetDriverIssues(int driverId)
        {
            var issues = await _context.DriverIssues
                .Where(d => d.DriverId == driverId)
                .Select(i => new DriverIssueDTO
                {
                    Id = i.Id,
                    DriverId = i.DriverId,
                    IssueId = i.IssueId,
                }).AsNoTracking().ToListAsync();
            return issues;
        }
       
        public async Task<DriverIssueDTO> GetDriverIssueById(int id)
        {
            var issue = await _context.DriverIssues
                .Where(i => i.Id == id)
                .Select(i => new DriverIssueDTO
                {
                    Id = i.Id,
                    DriverId = i.DriverId,
                    IssueId = i.IssueId,
                }).AsNoTracking().FirstOrDefaultAsync();
            if (issue == null) return null;
            return issue;
        }
        public async Task<int> AddIssueToDriver(DriverIssueDTO issueDTO)
        {
            DriverIssue issue = new DriverIssue
            {
                DriverId = issueDTO.DriverId,
                IssueId = issueDTO.IssueId,
            };
            await _context.AddAsync(issue);
            await _context.SaveChangesAsync();
            return issue.Id;
        }
        public async Task<bool> DeleteIssueFromDriver(int issueid)
        {
            var issue = await _context.DriverIssues
                .FirstOrDefaultAsync(i => i.Id == issueid);
            if (issue == null) return false;
            _context.Remove(issue);
            var roweffected = await _context.SaveChangesAsync();

            return (roweffected > 0);
        }

    }
}