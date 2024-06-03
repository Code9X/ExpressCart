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
    public class SubCategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public SubCategoryController(IUnitOfWork unitOfWork)
        {
			_unitOfWork= unitOfWork;
		}
        public IActionResult Index()
        {
			List<SubCategory> SubCategoryList = _unitOfWork.SubCategory.GetAll(null, c => c.Category).ToList();
			return View(SubCategoryList);
        }
		public IActionResult Upsert(int? id) 
		{
			SubCategoryVM subCategoryVM = new()
			{
				CategoryList = _unitOfWork.Category
				.GetAll(filter: null).Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
				SubCategory = new SubCategory()
			};

			if (id == 0 || id == null)
			{
				//Create
				return View(subCategoryVM);
			}
			else
			{
				//Update
				subCategoryVM.SubCategory = _unitOfWork.SubCategory.Get(u => u.Id == id);
				return View(subCategoryVM);
			}
		}
		[HttpPost]
		public IActionResult Upsert(SubCategoryVM subCategoryVM)
		{
			//if (ModelState.IsValid)
			//{
			if (subCategoryVM.SubCategory.Id == 0)
			{
				_unitOfWork.SubCategory.Add(subCategoryVM.SubCategory);
				TempData["success"] = "SubCategory Added";
			}
			else
			{
				_unitOfWork.SubCategory.Update(subCategoryVM.SubCategory);
				TempData["success"] = "SubCategory Updated";
			}

				_unitOfWork.Save();
			//}
			return RedirectToAction("Index");
		}
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            SubCategory subCategoryFromDb = _unitOfWork.SubCategory.Get(u => u.Id == id);
            if (subCategoryFromDb == null)
            {
                return NotFound();
            }
            return View(subCategoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
		public IActionResult DeletePOST(int? id)
		{
            SubCategory? obj = _unitOfWork.SubCategory.Get(u => u.Id == id);
			if (obj == null)
			{
				return NotFound();
			}
			_unitOfWork.SubCategory.Remove(obj);
			_unitOfWork.Save();
			TempData["success"] = "SubCategory Deleted";
			return RedirectToAction("Index");
		}

	}
}
