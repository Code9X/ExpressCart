using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using ExpressCart.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.Text;
using System.Security.Cryptography;
using ExpressCart.DataAccess.Repository.IRepository;
using Stripe.Checkout;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ExpressCart.DataAccess.Repository;

namespace ExpressCartWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAPIRepository _apiRepository;
		[BindProperty]
		public ShoppingCartVM shoppingCartVM { get; set; }
		public CartController(IUnitOfWork unitOfWork, IAPIRepository apiRepository)
		{
			_unitOfWork = unitOfWork;
            _apiRepository = apiRepository;
		}
        private string GetUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }
        public IActionResult Index()
		{
			var shoppingCartVM = new ShoppingCartVM
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == GetUserId(), p => p.Product),
				OrderHeader = new OrderHeader()
			};

			foreach (var cart in shoppingCartVM.ShoppingCartList)
			{
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
			}

			var productVM = new ProductVM
			{
				ProductImages = _unitOfWork.Product.GetAll(null, p => p.ProductImages).SelectMany(p => p.ProductImages).ToList()
			};

			var viewModel = new Tuple<ShoppingCartVM, ProductVM>(shoppingCartVM, productVM);

            Logging.LogAction(nameof(CartController), "Index page visited.", GetUserId());
            return View(viewModel);
		}
		public IActionResult Summary()
		{
			shoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == GetUserId(), includeProperties: u => u.Product),
				OrderHeader = new()
			};

			shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == GetUserId());
			shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
			shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
			shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
			shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var cart in shoppingCartVM.ShoppingCartList)
			{
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
			}

            Logging.LogAction(nameof(CartController), "Summary page visited.", GetUserId());
            return View(shoppingCartVM);
		}
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string action)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
            var name = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == GetUserId(),
                includeProperties: u => u.Product);

            if (shoppingCartVM.ShoppingCartList == null || !shoppingCartVM.ShoppingCartList.Any())
            {
                TempData["error"] = "No Products Added to the Cart";
                return RedirectToAction("Summary", "Cart");
            }

            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = GetUserId();

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }

            if (_unitOfWork.ApplicationUser.Get(u => u.Id == GetUserId()).CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            //OrderHeader is Created
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            string orderNumber = $"{DateTime.Now:yyyyMMdd}" + shoppingCartVM.OrderHeader.Id;
            shoppingCartVM.OrderHeader.OrderNo = orderNumber;

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            int orderHeaderId = shoppingCartVM.OrderHeader.Id;

            // OrderDetail is Created
            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = orderHeaderId,
					OrderNo = $"{DateTime.Now:yyyyMMdd}{shoppingCartVM.OrderHeader.Id}",
					Price = item.Product.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }

            // Generate Razorpay order
            string secretKey = _apiRepository.GetRazorSecretKey();
            string publishableKey = _apiRepository.GetRazorPublishableKey();

            RazorpayClient client = new RazorpayClient(secretKey, publishableKey);
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", shoppingCartVM.OrderHeader.OrderTotal * 100);
            options.Add("receipt", "order_rcptid_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            options.Add("currency", "INR");
            options.Add("payment_capture", "1"); // 1 - automatic  , 0 - manual

            Order orderResponse = client.Order.Create(options);
            string razorOrderId = orderResponse["id"].ToString();

            // Create RazorOrder model for return to the view
            RazorOrder razorOrder = new RazorOrder
            {
                orderId = razorOrderId,
                razorpayKey = secretKey,
                amount = shoppingCartVM.OrderHeader.OrderTotal * 100,
                currency = "INR",
                name = name,
                email = email,
                contactNumber = shoppingCartVM.OrderHeader.PhoneNumber,
                address = shoppingCartVM.OrderHeader.StreetAddress,
                description = "Test Mode"
            };

            _unitOfWork.Save();
            return View("PaymentPage", Tuple.Create(razorOrder, shoppingCartVM.OrderHeader.Id));
        }
        public IActionResult OrderConfirmation(int Id, string paymentStatus, string paymentId)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == Id, includeProperties: "ApplicationUser");

            if (paymentStatus == "paid")
            {
                orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                orderHeader.OrderStatus = SD.StatusApproved;
                orderHeader.PaymentIntentId = paymentId;
                orderHeader.PaymentDate = DateTime.Now;
				orderHeader.OrderCancelledYN = false;

				_unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();

                ViewBag.Id = Id;
                ViewBag.PaymentId = paymentId;

                Logging.LogAction(nameof(CartController), "OrderConfirmation page visited.", GetUserId());
                return View();
            }
            else 
            {
                orderHeader.PaymentStatus = SD.PaymentStatusPending;
                orderHeader.OrderStatus = SD.StatusPending;
				orderHeader.OrderCancelledYN = true;

				_unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                Logging.LogAction(nameof(CartController), "OrderCancelled page visited.", GetUserId());
                return View("OrderCancelled");
            }
		}
        public IActionResult OrderCancelled()
        {            
			return View();
        }
        public IActionResult Plus(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			cartFromDb.Count += 1;
			_unitOfWork.ShoppingCart.Update(cartFromDb);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
			if (cartFromDb.Count <= 1)
			{
				_unitOfWork.ShoppingCart.Remove(cartFromDb);
			}
			else
			{
				cartFromDb.Count -= 1;
				_unitOfWork.ShoppingCart.Update(cartFromDb);
			}

			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Remove(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
			_unitOfWork.ShoppingCart.Remove(cartFromDb);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
	}
}
