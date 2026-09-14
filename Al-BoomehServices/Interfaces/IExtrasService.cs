using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices.Interfaces
{
    public interface IExtrasService
    {
        Task<List<ExtraInfoDTO>> GetAllExtras(int pagenumber, int pagesize);
        Task<bool> IsExist(int id);
        Task<List<ExtraInfoDTO>> GetExtrasByProduct(int productId);
        Task<List<ExtraInfoDTO>> GetMandatoryExtras(int productId);
        Task<ExtraInfoDTO?> GetExtraById(int extraId);
        Task<int> CreateExtra(CreateExtraDTO extraDto);
        Task<bool> UpdateExtra(int extraId, UpdateExtraDTO extraDTO);
        Task<bool> DeleteExtra(int id);
        Task<Dictionary<int, int>> GetExtraCountPairsProduct();
        Task<bool> IsExtraExistByName(string name, int productId);
    }
}