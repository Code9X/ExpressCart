using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressCart.Models.ViewModels
{
	public class CompanyVM
	{
		public Company Company { get; set; }
		[ValidateNever]
		public IEnumerable<SelectListItem> CategoryList { get; set; }
		[ValidateNever]
		public IEnumerable<SelectListItem> SubCategoryList { get; set; }
	}
}
