using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices.Interfaces
{
    public interface IDriversService
    {
        Task<List<DriverInfoDTO>> GetAllDrivers(int pagenumber, int pagesize);
        Task<bool> IsExist(int id);
        Task<List<DriverInfoDTO>> GetDriversByVehicleType(enVehicleType VehicleType, int pagenumber, int pagesize);
        Task<DriverInfoDTO> GetDriverById(int driverId);
        Task<DriverInfoDTO> GetDriverByPhone(string phone);
        Task<int> CreateDriver(CreateDriverDTO driverDTO);
        Task<bool> UpdateDriverInfo(int driverId, UpdateDriverInfoDTO driverDTO);
        Task<bool> DeleteDriver(int id);
        Task<Dictionary<enVehicleType, int>> GetDriverCountPairsVehicleType();
        Task<bool> IsDriverExistByPhone(string phone);
        Task<List<DriverIssueDTO>> GetDriverIssues(int driverId);
        Task<DriverIssueDTO> GetDriverIssueById(int id);
        Task<int> AddIssueToDriver(DriverIssueDTO issueDTO);
        Task<bool> DeleteIssueFromDriver(int issueid);
    }
}