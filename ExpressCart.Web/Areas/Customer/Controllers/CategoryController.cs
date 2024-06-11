using System.Diagnostics;
using System.Security.Claims;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using ExpressCart.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Razorpay.Api;

namespace ExpressCartWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAPIRepository _apiRepository;
        public CategoryController(ILogger<CategoryController> logger, IUnitOfWork unitOfWork, IAPIRepository apiRepository)
        {
            _logger = logger;
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
        public IActionResult Index(int? categoryId, string searchTerm = null)
        {
            // Retrieve the category
            var category = _unitOfWork.Category
                .GetAll(p => p.Id == categoryId)
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
                .ToList();


            // Retrieve the subcategories for the given categoryId
            var subcategories = _unitOfWork.SubCategory
                .GetAll(p => p.CategoryID == categoryId)
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
                .ToList();

            // Store CategoryName and CategoryId when index page is loaded
            string storedcategoryName = null;
            int? storedCategoryId = null;
            if (category.Any())
            {
                storedcategoryName = category.First().Text;
                storedCategoryId = Convert.ToInt32(category.First().Value);
            }

            ViewBag.ControllerName = "Category";
            ViewBag.CategoryName = storedcategoryName;
            ViewBag.CategoryId = storedCategoryId;

            // Retrieve category images
            var categoryImages = _unitOfWork.Category
                .GetAll(
                    null,
                    p => p.CategoryImages)
                .Select(p => new CategoryVM
                {
                    Category = p,
                    CategoryImages = p.CategoryImages
                })
                .ToList();

            // Filter products by categoryId and searchTerm
            var productsQuery = _unitOfWork.Product
                .GetAll(
                    p => p.CategoryID == categoryId && (string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm)),
                    p => p.Company,
                    p => p.ProductImages)
                .Select(u => new SelectListItem
                {
                    Text = $"{u.Name} - {u.Company.Name} - {u.CategoryID}",
                    Value = $"{u.Id}:{u.Price}:{u.SubCategoryID}" // Include subcategory ID in the Value
                });

            // Retrieve product images
            var productImages = _unitOfWork.Product
                .GetAll(
                    p => p.CategoryID == categoryId && (string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm)), // Filter by categoryId and searchTerm
                    p => p.ProductImages)
                .Select(p => new ProductVM
                {
                    Product = p,
                    ProductImages = p.ProductImages
                })
                .ToList();

            var productVM = new ProductVM
            {
                CategoryList = category,
                ProductList = productsQuery,
                ProductImages = productImages.SelectMany(vm => vm.ProductImages).ToList(), // Combine all product images
                CategoryImages = categoryImages.SelectMany(vm => vm.CategoryImages).ToList()
            };

            var categoryVM = new CategoryVM
            {
                CategoryList = category,
                SubCategoryList = subcategories
            };

            var viewModel = new Tuple<ProductVM, CategoryVM>(productVM, categoryVM);

            Logging.LogAction(nameof(CategoryController), "Index page visited.", GetUserId());

            if (categoryVM.CategoryList.FirstOrDefault()?.Text != "Travel")
                return View(viewModel);
            else
                return RedirectToAction("Index", "Travel", new { categoryId = categoryId, searchTerm = searchTerm });
        }
        public IActionResult Details(int productId)
        {
            var product = _unitOfWork.Product
             .Get(p => p.Id == productId, includeProperties: "Company,ProductImages,Category");

            if (product == null)
            {
                return NotFound();
            }

            var productVM = new ProductVM
            {
                Product = product,
                Category = product.Category,
                ProductImages = product.ProductImages,
            };

            Logging.LogAction(nameof(CategoryController), "Details page visited.", GetUserId());
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

                TempData["success"] = "Added to Cart";

                return Redirect($"/Customer/Category/Details?productId={shoppingCart.ProductId}&actionType={actionType}");

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

            Logging.LogAction(nameof(CategoryController), "Summary page visited.", GetUserId());
            return View("Summary", shoppingCartVM);
        }
		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost(string action, ShoppingCartVM shoppingCartVM, int count)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
            var name = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

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

                Logging.LogAction(nameof(CategoryController), "OrderConfirmation page visited.", GetUserId());
                return View();
            }
            else
            {
                orderHeader.PaymentStatus = SD.PaymentStatusPending;
                orderHeader.OrderStatus = SD.StatusPending;
                orderHeader.OrderCancelledYN = true;

                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                Logging.LogAction(nameof(CategoryController), "OrderCancelled page visited.", GetUserId());
                return View("OrderCancelled");
            }
        }
    }
}
