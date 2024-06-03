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
    public class AddvertisementImage
	{
		public int Id { get; set; }
		[Required]
		public string ImageUrl { get; set; }
		public int AdId { get; set; }
		[ForeignKey("AdId")]
		public Addvertisement Addvertisement { get; set; }

	}
}
