using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressCart.Models.ViewModels
{
	public class CategoryVM
	{
		public Category Category { get; set; }
		public IEnumerable<SelectListItem> CategoryList { get; set; }
		public IEnumerable<SelectListItem> SubCategoryList { get; set; }
		public List<CategoryImage> CategoryImages { get; set; }

	}
}
