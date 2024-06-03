using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressCart.Models.ViewModels
{
	public class ProductVM
	{
		public Product Product { get; set; }
		public Category Category { get; set; }
		public List<ProductImage> ProductImages { get; set; }
		public List<CategoryImage> CategoryImages { get; set; }

		[ValidateNever]
		public IEnumerable<SelectListItem> CategoryList { get; set; }

		[ValidateNever]
		public IEnumerable<SelectListItem> SubCategoryList { get; set; }

		[ValidateNever]
		public IEnumerable<SelectListItem> CompanyList { get; set; }

		[ValidateNever]
		public IEnumerable<SelectListItem> ProductList { get; set; }
	}
}
