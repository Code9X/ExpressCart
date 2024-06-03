using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpressCart.Models;

public class Company
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
    public string? Address { get; set; }

    [DisplayName("Display Order")]
    [Range(0, 100, ErrorMessage = "Display Order must be between 1-100")]
    public int? DisplayOrder { get; set; }
}
