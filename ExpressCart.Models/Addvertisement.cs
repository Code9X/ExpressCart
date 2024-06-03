using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ExpressCart.Models
{
    public class Addvertisement
	{
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Addvertisement Name")]
        [MaxLength(30)]
        public string? Name { get; set; }

        [DisplayName("Display Order")]
        [Range(0, 100, ErrorMessage = "Display Order must be between 1-100")]
        public int DisplayOrder { get; set; }
		[ValidateNever]
		public List<AddvertisementImage> AddvertisementImages { get; set; }
	}
}
