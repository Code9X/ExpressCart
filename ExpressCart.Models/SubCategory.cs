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
    public class SubCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Sub-Category Name")]
        [MaxLength(30)]
        public string? Name { get; set; }

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; }

        [DisplayName("Display Order")]
        [Range(0, 100, ErrorMessage = "Display Order must be between 1-100")]
        public int DisplayOrder { get; set; }
    }
}
