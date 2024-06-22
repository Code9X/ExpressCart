using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressCart.Models.ViewModels
{
	public class TravelVM
	{
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<AirportDetails> AirportData { get; set; }
        public string RandomAirportCode { get; set; }
        public string RandomAirportName { get; set; }
        public Travel Travel { get; set; }
    }
}
