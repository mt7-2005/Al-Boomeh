using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderInfoDTO>> GetAllOrders(int pagenumber, int pagesize);
        Task<long> GetOrdersByStatus(enStatus Status,int storeId);
        Task<OrderInfoDTO> GetOrderById(int orderId);
        Task<OrderInfoDTO> GetOrderByCode(string orderCode);
        Task<int> CreateCart(string idempotencyKey, CreateOrderCartDTO orderDTO);
        Task<bool> PlaceOrder(int orderId, PlaceOrderDTO orderDTO, decimal deliveryfees = 0, decimal servicefees = 0);
        Task<bool> UpdateOrderStatus(int orderId, UpdateOrderStatusDTO statusdto);
        Task<bool> UpdateOrderTime(int orderId, UpdateOrderTimeDTO orderTimeDTO);
        Task<bool> UpdateOrderDriver(int orderId, UpdateDriverDTO updateDriverDTO);
        Task<bool> DeleteOrder(int id);
        Task<List<OrderLineDTO>> GetOrderLines(string orderCode);
        Task<int> AddOrderLine(int orderId, CreateOrderLineDTO lineDTO);
        Task<int> AddLineToCart(int orderId, CreateOrderLineDTO lineDTO);
        Task<bool> DeleteOrderLine(int Id);
        Task<bool> DeleteLineFromCart(int id);
        Task<Dictionary<enStatus, int>> GetOrderCountPairsStatus();
        Task<long> GetOrderCount();
        Task<int> CreateOrderFeedback(OrderFeedbackDTO feedbackDTO);
        Task<OrderFeedbackDTO> GetOrderFeedback(int orderid);
        Task<bool> IsOrderExistByCode(string code);
        Task<decimal> AvgOrderValueLastMonth();
        Task<List<OrderStatusHistoryDTO>?> GetOrderStatusHistories(int orderid);
        Task<OrderLineDTO?> GetOrderLine(int id);
        Task<bool> IsOrderLineExist(int id);
        Task<bool> IsExist(int id);


    }
}
