using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ExpressCart.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("Category")]
        public int CategoryID { get; set; }
        public Category Category { get; set; }

        [ForeignKey("SubCategory")]
        public int? SubCategoryID { get; set; }
        public SubCategory SubCategory { get; set; }

        [ForeignKey("Company")]
        public int? CompanyID { get; set; }
        public Company Company { get; set; }

        public double Price { get; set; }
        public double? OfferPrice { get; set; }
		public string Specifications { get; set; }
		public string Rating { get; set; }

		[DisplayName("Display Order")]
        [Range(0, 100, ErrorMessage = "Display Order must be between 1-100")]
        public int? DisplayOrder { get; set; }
        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; }
    }
}
