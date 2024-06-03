using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpressCart.Models
{
    public class CategoryImage
    {
		public int Id { get; set; }
		[Required]
		public string ImageUrl { get; set; }
		public int CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		public Category Category { get; set; }

	}
}
