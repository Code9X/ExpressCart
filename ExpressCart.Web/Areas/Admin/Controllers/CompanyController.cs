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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> CompanyList = _unitOfWork.Company.GetAll(null,
                c => c.Category,
                c => c.SubCategory).ToList();
            return View(CompanyList);
        }
        public IActionResult Upsert(int? id)
        {
            CompanyVM companyVM = new()
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
				Company = new Company()
            };

            if (id == 0 || id == null)
            {
                //Create
                return View(companyVM);
            }
            else
            {
                //Update
                companyVM.Company = _unitOfWork.Company.Get(u => u.Id == id);
                return View(companyVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(CompanyVM companyVM)
        {
            //if (ModelState.IsValid)
            //{
            if (companyVM.Company.Id == 0)
            {
                _unitOfWork.Company.Add(companyVM.Company);
				TempData["success"] = "Company Added";
			}
            else
            {
                _unitOfWork.Company.Update(companyVM.Company);
				TempData["success"] = "Company Updated";
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
            Company companyFromDb = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyFromDb == null)
            {
                return NotFound();
            }
            return View(companyFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Company? obj = _unitOfWork.Company.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
			TempData["success"] = "Company Deleted";
			return RedirectToAction("Index");
        }
    }
}
