using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;

namespace ExpressCartWeb.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class TravelController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAPIRepository _apiRepository;
        private readonly HttpClient client;
        private string GetUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }
        public TravelController(IAPIRepository apiRepository, IUnitOfWork unitOfWork)
        {
            _apiRepository = apiRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(int categoryId)
        {
            var category = _unitOfWork.Category.Get(u => u.Id == categoryId);

            List<AirportDetails> airportData = await GetAirportDtlsAsync();

            // Select a random airport
            var random = new Random();
            int index = random.Next(airportData.Count);
            var randomAirport = airportData[index];

            var travelvm = new TravelVM
            {
                CategoryId = categoryId,
                CategoryName = category.Name,
                AirportData = airportData,
                RandomAirportCode = randomAirport.Code,
                RandomAirportName = randomAirport.Name,
                Travel = new Travel()
            };

            return View(travelvm);
        } 
        [HttpPost]
        public IActionResult Index(Travel travel)
        {
            travel.UserId = GetUserId();

            if (travel.Id == 0)
            {
                _unitOfWork.Travel.Add(travel);
                TempData["success"] = "Travel details added successfully.";
            }
            else
            {
                _unitOfWork.Travel.Update(travel);
                TempData["success"] = "Travel details updated successfully.";
            }
            _unitOfWork.Save();
            return RedirectToAction("Index","Home");
        }
        private async Task<List<AirportDetails>> GetAirportDtlsAsync()
        {
            return await Task.FromResult(new List<AirportDetails>
            {
                new AirportDetails { Code = "JFK", Name = "John F. Kennedy International Airport" },
                new AirportDetails { Code = "LAX", Name = "Los Angeles International Airport" },
                new AirportDetails { Code = "ORD", Name = "O'Hare International Airport" }
            });
        }

        //private async Task<List<AirportDetails>> GetAirportDtls()
        //{
        //    using (var client = new HttpClient()) //A HttpClient object is created inside a using statement to ensure it is properly disposed of after use.
        //    {
        //        string countryAPIUrl = _apiRepository.GetCountryApiUrl();
        //        string apiKey = _apiRepository.GetCountryApiKey();

        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
        //        client.DefaultRequestHeaders.Add("x-rapidapi-host", "booking-com15.p.rapidapi.com");

        //        HttpResponseMessage response = await client.GetAsync(countryAPIUrl);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            string json = await response.Content.ReadAsStringAsync();
        //            dynamic jsonResponse = JsonConvert.DeserializeObject(json);

        //            List<AirportDetails> airports = new List<AirportDetails>();

        //            foreach (var item in jsonResponse.data)
        //            {
        //                airports.Add(new AirportDetails
        //                {
        //                    // Concatenating city name and code
        //                    Code = (string)item.cityName,
        //                    Name = $"{(string)item.name} - {(string)item.code}"
        //                });
        //            }

        //            return airports;
        //        }

        //        return new List<AirportDetails>();
        //    }
        //}

    }
}