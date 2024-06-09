using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressCart.Models.ViewModels
{
	public class TravelVM
	{
        public Travel Travel { get; set; }
		public int SelectedCityId { get; set; }
		public IEnumerable<SelectListItem> CityList { get; set; }
	}
}
