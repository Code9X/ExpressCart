using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using ExpressCart.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpressCartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public CategoryController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
			_unitOfWork= unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}
        public IActionResult Index()
        {
            List<Category> CategoryList = _unitOfWork.Category.GetAll(filter: null).ToList();
            return View(CategoryList);
        }
		public IActionResult Upsert(int? id)
		{
			CategoryVM categoryVM = new CategoryVM
			{
				CategoryList = _unitOfWork.Category
					.GetAll(filter: null)
					.Select(u => new SelectListItem
					{
						Text = u.Name,
						Value = u.Id.ToString()
					}),
				Category = new Category()
			};

			if (id == 0 || id == null)
			{
				//Create
				return View(categoryVM);
			}
			else
			{
				//Update
				categoryVM.Category = _unitOfWork.Category.Get(u => u.Id == id);
				return View(categoryVM);
			}
		}


		[HttpPost]
		public IActionResult Upsert(CategoryVM categoryVM, List<IFormFile> files)
		{
			//if (ModelState.IsValid)
			//{
			if (categoryVM.Category.Id == 0)
			{
				_unitOfWork.Category.Add(categoryVM.Category);
				TempData["success"] = "Category Added";
			}
			else
			{
				_unitOfWork.Category.Update(categoryVM.Category);
				TempData["success"] = "Category Updated";
			}

				_unitOfWork.Save();
			//}

			string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (files != null)
				{
					foreach (IFormFile file in files)
					{
						string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						string categoryPath = @"images\categories\category-" + categoryVM.Category.Id;
						string finalPath = Path.Combine(wwwRootPath, categoryPath);

						if (!Directory.Exists(finalPath))
							Directory.CreateDirectory(finalPath);

						using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
						{
							file.CopyTo(fileStream);
						}

						CategoryImage categoryImage = new()
						{
							ImageUrl = @"\" + categoryPath + @"\" + fileName,
							CategoryId = categoryVM.Category.Id,
						};

						if (categoryVM.Category.CategoryImages == null)
							categoryVM.Category.CategoryImages = new List<CategoryImage>();

						categoryVM.Category.CategoryImages.Add(categoryImage);

					}
					_unitOfWork.Category.Update(categoryVM.Category);
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
			Category categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
			if (categoryFromDb == null)
			{
				return NotFound();
			}
			return View(categoryFromDb);
		}
		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePOST(int? id)
		{
			Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
			if (obj == null)
			{
				return NotFound();
			}
			_unitOfWork.Category.Remove(obj);
			_unitOfWork.Save();
			TempData["success"] = "Category Deleted";
			return RedirectToAction("Index");
		}
	}
}
