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
        public string DepLoc { get; set; }
        public string DepTerm { get; set; }
        public string DepDate { get; set; }
        public int Stops { get; set; }
        public string DestLoc { get; set; }
        public string DestTerm { get; set; }
        public string DestDate { get; set; }
        public int Adults { get; set; }
        public int Childerns { get; set; }
        public string CarrierName  { get; set; }
        public string AircraftName { get; set; }
        public string Code { get; set; }
        public bool OneWay { get; set; }
        public string BasePrice { get; set; }
        public string TotalPrice { get; set; }
    }
    public class CityAndAirport
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DetailedName { get; set; }
    }
}
