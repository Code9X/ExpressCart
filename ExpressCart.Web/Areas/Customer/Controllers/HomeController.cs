using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using ExpressCart.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Razorpay.Api;
using Stripe.Checkout;

namespace ExpressCartWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;
		private string UserId;
        public ShoppingCartVM shoppingCartVM { get; set; }
		public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
			_configuration = configuration;
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
        public IActionResult Index(string searchTerm)
		{            
            ViewBag.SearchTerm = searchTerm;

            IEnumerable<Product> productsQuery;

			if (!string.IsNullOrEmpty(searchTerm))
			{
				productsQuery = _unitOfWork.Product.GetAll(
					filter: p => p.Name.Contains(searchTerm),
					includeProperties: new Expression<Func<Product, object>>[] { p => p.Company, p => p.ProductImages });
			}
			else
			{
				productsQuery = _unitOfWork.Product.GetAll(null,
					includeProperties: new Expression<Func<Product, object>>[] { p => p.Company, p => p.ProductImages });
			}

			var productList = productsQuery.ToList();

			var categoryImages = _unitOfWork.Category.GetAll(
				null,
				p => p.CategoryImages)
				.SelectMany(p => p.CategoryImages)
				.ToList();

			var productVM = CreateProductViewModel(productList, categoryImages);

			var viewModel = new Tuple<ProductVM, AdvertisementVM>(productVM, CreateAdvertisementViewModel());

            Logging.LogAction(nameof(HomeController), "Index page visited.", GetUserId());
            return View(viewModel);
		}		
		public IActionResult Details(int productId)
		{
			var product = _unitOfWork.Product
				.Get(
					p => p.Id == productId,
					includeProperties: "Company,ProductImages");

			if (product == null)
			{
				return NotFound();
			}

			var productVM = new ProductVM 
			{
				Product = product,
				ProductImages = product.ProductImages
			};

            Logging.LogAction(nameof(HomeController), "Details page visited.", GetUserId());
            return View(productVM);
		}
		[HttpPost]
		[Authorize]
		public IActionResult Details(ShoppingCart shoppingCart, string actionType)
		{
			shoppingCart.ApplicationUserId = GetUserId();

			if (actionType == "BuyNow")
			{
				return Summary(shoppingCart);
			}
			else
			{
				ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == GetUserId() &&
																					   u.ProductId == shoppingCart.ProductId);
				if (cartFromDB != null)
				{
					// Shopping Cart already exists
					cartFromDB.Count += shoppingCart.Count;
					_unitOfWork.ShoppingCart.Update(cartFromDB);
					_unitOfWork.Save();
				}
				else
				{
					// Add Shopping Cart
					_unitOfWork.ShoppingCart.Add(shoppingCart);
					_unitOfWork.Save();
				}

				TempData["success"] = "Added To Cart";

                Logging.LogAction(nameof(HomeController), "Details Post Action.", GetUserId());
                return RedirectToAction(nameof(Index));
			}
		}
		public IActionResult Summary(ShoppingCart shoppingCart)
		{
			shoppingCart.ApplicationUserId = GetUserId();

			var product = _unitOfWork.Product.Get(
				p => p.Id == shoppingCart.ProductId,
				includeProperties: "Company,ProductImages");

			shoppingCart.Product = product; // Associate the product with the shopping cart

			var shoppingCartVM = new ShoppingCartVM
			{
				ShoppingCartList = new List<ShoppingCart> { shoppingCart },
				OrderHeader = new OrderHeader(),
				ProductId = shoppingCart.ProductId
			};
			shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == GetUserId());
			shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
			shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
			shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
			shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var Item in shoppingCartVM.ShoppingCartList)
			{
				shoppingCartVM.OrderHeader.OrderTotal += (Item.Product.Price * Item.Count);
			}

            Logging.LogAction(nameof(HomeController), "Summary page visited.", GetUserId());
            return View("Summary", shoppingCartVM);
		}
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string action, ShoppingCartVM shoppingCartVM, int count)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
			var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
			var name = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

			ApplicationUser ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == GetUserId());

            // Populate OrderHeader properties
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = GetUserId();
           
            // Determine payment and order status based on user type 
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == GetUserId());
            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                // Regular Customer Account
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                // Company Account
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            // Add order and its details to the database
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			string orderNumber = $"{DateTime.Now:yyyyMMdd}" + shoppingCartVM.OrderHeader.Id;
            shoppingCartVM.OrderHeader.OrderNo = orderNumber;

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            var product = _unitOfWork.Product.Get(p => p.Id == shoppingCartVM.ProductId);

            OrderDetail orderDetail = new OrderDetail
            {
                ProductId = shoppingCartVM.ProductId,
                OrderHeaderId = shoppingCartVM.OrderHeader.Id,
				OrderNo = $"{DateTime.Now:yyyyMMdd}{shoppingCartVM.OrderHeader.Id}",
				Product = product,
                Price = product.Price,
                Count = count
            };
            shoppingCartVM.OrderHeader.OrderTotal = orderDetail.Price * count;

            _unitOfWork.OrderDetail.Add(orderDetail);
			_unitOfWork.Save();

			// Generate Razorpay order
			string secretKey = _configuration["Razor:SecretKey"];
			string publishableKey = _configuration["Razor:PublishableKey"];

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
            OrderHeader orderHeader = _unitOfWork.OrderHeader
			.Get(u => u.Id == Id, includeProperties: "ApplicationUser");

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

                Logging.LogAction(nameof(HomeController), "OrderConfirmation page visited.", GetUserId());
                return View();
			}
			else
			{
				orderHeader.PaymentStatus = SD.PaymentStatusPending;
				orderHeader.OrderStatus = SD.StatusPending;
				orderHeader.OrderCancelledYN = true;

				_unitOfWork.OrderHeader.Update(orderHeader);
				_unitOfWork.Save();

                Logging.LogAction(nameof(HomeController), "OrderCancelled page visited.", GetUserId());
                return View("OrderCancelled");
			}
		}
		private ProductVM CreateProductViewModel(IEnumerable<Product> products, List<CategoryImage> categoryImages)
        {
            var category = _unitOfWork.Category.GetAll(filter: null)
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            var productList = products
                .Select(u => new SelectListItem
                {
                    Text = $"{u.Name} - {u.Company.Name}",
                    Value = $"{u.Id}:{u.Price}"
                })
                .ToList();

            var productImages = products
                .SelectMany(p => p.ProductImages)
                .ToList();

            var productVM = new ProductVM
            {
                CategoryList = category,
                ProductList = productList,
                ProductImages = productImages,
                CategoryImages = categoryImages // Ensure categoryImages is a List<CategoryImage>
            };

            return productVM;
        }
        private AdvertisementVM CreateAdvertisementViewModel()
        {
            var advertisements = _unitOfWork.Addvertisement.GetAll(
                null,
                p => p.AddvertisementImages)
                .Select(u => new SelectListItem
                {
                    Text = $"{u.Name}",
                    Value = $"{u.Id}"
                });

            var advertisementImages = _unitOfWork.Addvertisement.GetAll(
                null,
                a => a.AddvertisementImages)
                .Select(a => new AdvertisementVM
                {
                    Addvertisement = a,
                    AddvertisementImages = a.AddvertisementImages
                })
                .ToList();

            var advertisementVM = new AdvertisementVM
            {
                AddvertisementList = advertisements,
                AddvertisementImages = advertisementImages.SelectMany(vm => vm.AddvertisementImages).ToList()
            };

            return advertisementVM;
        }
    }
}
