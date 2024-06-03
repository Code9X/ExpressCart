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
    public class AdController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public AdController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
			_unitOfWork= unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}
        public IActionResult Index()
        {
            List<Addvertisement> AddList = _unitOfWork.Addvertisement.GetAll(filter: null).ToList();
            return View(AddList);
        }
		public IActionResult Upsert(int? id)
		{
			AdvertisementVM advertisementVM = new AdvertisementVM
			{
				AddvertisementList = _unitOfWork.Addvertisement
					.GetAll(filter: null)
					.Select(u => new SelectListItem
					{
						Text = u.Name,
						Value = u.Id.ToString()
					}),
				Addvertisement = new Addvertisement()
			};

			if (id == 0 || id == null)
			{
				//Create
				return View(advertisementVM);
			}
			else
			{
				//Update
				advertisementVM.Addvertisement = _unitOfWork.Addvertisement.Get(u => u.Id == id);
				return View(advertisementVM);
			}
		}


		[HttpPost]
		public IActionResult Upsert(AdvertisementVM advertisementVM, List<IFormFile> files)
		{
			//if (ModelState.IsValid)
			//{
			if (advertisementVM.Addvertisement.Id == 0)
			{
				_unitOfWork.Addvertisement.Add(advertisementVM.Addvertisement);
				TempData["success"] = "Addvertisement Added";
			}
			else
			{
				_unitOfWork.Addvertisement.Update(advertisementVM.Addvertisement);
				TempData["success"] = "Addvertisement Updated";
			}

				_unitOfWork.Save();
			//}

			string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (files != null)
				{
					foreach (IFormFile file in files)
					{
						string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						string advertisementPath = @"images\advertisements\advertisement-" + advertisementVM.Addvertisement.Id;
						string finalPath = Path.Combine(wwwRootPath, advertisementPath);

						if (!Directory.Exists(finalPath))
							Directory.CreateDirectory(finalPath);

						using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
						{
							file.CopyTo(fileStream);
						}

						AddvertisementImage addvertisementImage = new()
						{
							ImageUrl = @"\" + advertisementPath + @"\" + fileName,
							AdId = advertisementVM.Addvertisement.Id,
						};

						if (advertisementVM.Addvertisement.AddvertisementImages == null)
						advertisementVM.Addvertisement.AddvertisementImages = new List<AddvertisementImage>();

					advertisementVM.Addvertisement.AddvertisementImages.Add(addvertisementImage);

					}
					_unitOfWork.Addvertisement.Update(advertisementVM.Addvertisement);
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
			Addvertisement AdFromDb = _unitOfWork.Addvertisement.Get(u => u.Id == id);
			if (AdFromDb == null)
			{
				return NotFound();
			}
			return View(AdFromDb);
		}
		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePOST(int? id)
		{
			Addvertisement? obj = _unitOfWork.Addvertisement.Get(u => u.Id == id);
			if (obj == null)
			{
				return NotFound();
			}
			_unitOfWork.Addvertisement.Remove(obj);
			_unitOfWork.Save();
			TempData["success"] = "Addvertisement Deleted";
			return RedirectToAction("Index");
		}
	}
}
