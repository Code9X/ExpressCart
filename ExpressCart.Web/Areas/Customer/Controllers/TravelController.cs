using System.Net.Http.Headers;
using System.Text.Json;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;

namespace ExpressCartWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class TravelController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpClient client;
        public TravelController(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            List<AirportDetails> airportData = await GetAirportDtlsAsync();
            return View(airportData);
        }
        private async Task<List<AirportDetails>> GetAirportDtlsAsync()
        {
            using (var client = new HttpClient())
            {
                string countryAPIUrl = _configuration["CountryAPI:countryApiUrl"];
                string apiKey = _configuration["CountryAPI:ApiKey"];

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
                client.DefaultRequestHeaders.Add("x-rapidapi-host", "booking-com15.p.rapidapi.com");

                HttpResponseMessage response = await client.GetAsync(countryAPIUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(json);

                    List<AirportDetails> airports = new List<AirportDetails>();

                    foreach (var item in jsonResponse.data)
                    {
                        airports.Add(new AirportDetails
                        {
                            // Concatenating city name and code
                            Code = (string)item.cityName,
                            Name = $"{(string)item.name} - {(string)item.code}"
                        });
                    }

                    return airports;
                }

                return new List<AirportDetails>();
            }
        }

    }
}