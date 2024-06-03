using System.Linq.Expressions;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using ExpressCart.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressCartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}
        public IActionResult Index()
        {
            List<Product> ProductList = _unitOfWork.Product.GetAll(null,
                c => c.Category,
                c => c.SubCategory,
                c => c.Company).ToList();
            return View(ProductList);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category
                .GetAll(filter: null).Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
				SubCategoryList = _unitOfWork.SubCategory
				.GetAll(filter: null).Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
				CompanyList = _unitOfWork.Company
				.GetAll(filter: null).Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
				Product = new Product()
            };

            if (id == 0 || id == null)
            {
                //Create
                return View(productVM);
            }
            else
            {
				//Update
				productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM,List<IFormFile> files)
        {
            //if (ModelState.IsValid)
            //{
            if (productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
				TempData["success"] = "Product Added";
			}
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
				TempData["success"] = "Product Updated";
			}

            _unitOfWork.Save();
            //}

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if(files !=null)
            {
				foreach (IFormFile file in files)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = @"images\products\product-" + productVM.Product.Id;
					string finalPath = Path.Combine(wwwRootPath, productPath);

					if (!Directory.Exists(finalPath))
						Directory.CreateDirectory(finalPath);

					using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}

					ProductImage productImage = new()
					{
						ImageUrl = @"\" + productPath + @"\" + fileName,
						ProductId = productVM.Product.Id,
					};

					if (productVM.Product.ProductImages == null)
						productVM.Product.ProductImages = new List<ProductImage>();

					productVM.Product.ProductImages.Add(productImage);

				}

				_unitOfWork.Product.Update(productVM.Product);
				_unitOfWork.Save();
			}
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
			Product productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
			TempData["success"] = "Product Deleted";
			return RedirectToAction("Index");
        }
    }
}
