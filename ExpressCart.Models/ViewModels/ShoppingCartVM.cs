using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressCart.Models;

namespace ExpressCart.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public OrderDetail OrderDetail { get; set; }
		public int ProductId { get; set; }
		public string Key { get; set; }
		public string OrderId { get; set; }
		public string Currency { get; set; }
		public int Payment_Capture { get; set; }
		public string PaymentId { get; set; }
		public string Signature { get; set; }
    }
}
