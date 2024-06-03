using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int Id,string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentId(int Id,string sessionId, string paymentIntentId);
    }
}
