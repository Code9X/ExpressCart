using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCart.Models
{
    public class Travel
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string DepartureLocation { get; set; }
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }
        public string DestinationLocation { get; set; }
        [DataType(DataType.Date)]
        public DateTime DestinationDate { get; set; }
        public int Adults_count { get; set; }
        public int Childerns_count { get; set; }
    }
    public class CityAndAirport
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
