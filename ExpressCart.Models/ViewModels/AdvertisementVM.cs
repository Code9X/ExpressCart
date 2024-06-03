using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressCart.Models.ViewModels
{
	public class AdvertisementVM
	{
		public Addvertisement Addvertisement { get; set; }
		public IEnumerable<SelectListItem> AddvertisementList { get; set; }
		public List<AddvertisementImage> AddvertisementImages { get; set; }

	}
}
