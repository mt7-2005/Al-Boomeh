using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IAnalyticsService
    {
        Task<bool> UpdateAnalytic(int storeId, decimal totalRevenue, Guid messageId);
        Task<AnalyticsDTO?> GetAnalytics(int storeId);
        Task<List<AnalyticsDTO>?> GetAllAnalytics();
    }
}
