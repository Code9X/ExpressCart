using System.Drawing.Text;
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
        public async Task<IActionResult> Index(TravelVM travelvm)
        {
            var departureLocation = travelvm.Travel.DepartureLocation.ToString();
            var destinationLocation = travelvm.Travel.DestinationLocation.ToString();
            var departureDate = travelvm.Travel.DepartureDate.ToString("yyyy-MM-dd");
            var destinationDate = travelvm.Travel.DestinationDate.ToString("yyyy-MM-dd");
            var adultsCount = travelvm.Travel.Adults_count.ToString();
            var childrenCount = travelvm.Travel.Childerns_count.ToString();
            var maxPrice = travelvm.MaxPrice.ToString();
            var maxCount = travelvm.MaxCount.ToString();
            var currencyCode = travelvm.CurrencyCode.ToString();
            var travelClass = travelvm.Class.ToString();
            bool nonStop = travelvm.NonStop;

            // Call GetFlightDetailsAsync with parameters
            List<FlightData> flightDetails = await GetFlightDetailsAsync(
                departureLocation,
                destinationLocation,
                departureDate,
                destinationDate,
                int.Parse(adultsCount),
                int.Parse(childrenCount),
                travelClass,
                nonStop,
                currencyCode,
                double.Parse(maxPrice),
                int.Parse(maxCount)
            );

            return View(flightDetails);
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

        private async Task<List<FlightData>> GetFlightDetailsAsync(string originLocCode, string destinationLocCode, string depDate, string returnDate, int adults, int children, 
                                                                   string travelClass, bool nonStop, string currencyCode,double maxPrice,int maxCount)
        {
            string baseUrl = _apiRepository.GetFlightApiUrl();
            string apiKey = _apiRepository.GetFlightApiKey();

            string formattedUrl = string.Format(baseUrl,originLocCode,destinationLocCode,depDate,returnDate,adults,children,travelClass, nonStop.ToString().ToLower(), currencyCode,maxPrice,maxCount
     );

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                //client.DefaultRequestHeaders.Add("X-API-Key", "69LPLStrOd6oQWMhhzqmvyTUNrg88TyS");
                //client.DefaultRequestHeaders.Add("X-API-Secret", "Kae8dxxOypIRDbRL");

                HttpResponseMessage response = await client.GetAsync(formattedUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var root = JsonConvert.DeserializeObject<Root>(json);

                    return root.Data;
                }
                else
                {
                    string message = response.StatusCode.ToString();
                    TempData["success"] = message;
                }

                return new List<FlightData>();
            }
        }


    }

    public class Root
    {
        public Meta Meta { get; set; }
        public List<FlightData> Data { get; set; }
    }

    public class Meta
    {
        // Define properties according to the JSON structure
    }

    public class FlightData
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public bool InstantTicketingRequired { get; set; }
        public bool NonHomogeneous { get; set; }
        public bool OneWay { get; set; }
        public bool IsUpsellOffer { get; set; }
        public DateTime LastTicketingDate { get; set; }
        public int NumberOfBookableSeats { get; set; }
        public List<Itinerary> Itineraries { get; set; }
        public Price Price { get; set; }
        public PricingOptions PricingOptions { get; set; }
        public List<string> ValidatingAirlineCodes { get; set; }
        public List<TravelerPricing> TravelerPricings { get; set; }
    }

    public class Itinerary
    {
        public string Duration { get; set; }
        public List<Segment> Segments { get; set; }
    }

    public class Segment
    {
        public Departure Departure { get; set; }
        public Arrival Arrival { get; set; }
        public string CarrierCode { get; set; }
        public string Number { get; set; }
        public Aircraft Aircraft { get; set; }
        public Operating Operating { get; set; }
        public string Duration { get; set; }
        public string Id { get; set; }
        public int NumberOfStops { get; set; }
        public bool BlacklistedInEU { get; set; }
    }

    public class Departure
    {
        public string IataCode { get; set; }
        public string Terminal { get; set; }
        public DateTime At { get; set; }
    }

    public class Arrival
    {
        public string IataCode { get; set; }
        public DateTime At { get; set; }
    }

    public class Aircraft
    {
        public string Code { get; set; }
    }

    public class Operating
    {
        public string CarrierCode { get; set; }
    }

    public class Price
    {
        public string Currency { get; set; }
        public string Total { get; set; }
        public string Base { get; set; }
        public List<Fee> Fees { get; set; }
        public string GrandTotal { get; set; }
    }

    public class Fee
    {
        public string Amount { get; set; }
        public string Type { get; set; }
    }

    public class PricingOptions
    {
        public List<string> FareType { get; set; }
        public bool IncludedCheckedBagsOnly { get; set; }
    }

    public class TravelerPricing
    {
        public string TravelerId { get; set; }
        public string FareOption { get; set; }
        public string TravelerType { get; set; }
        public Price Price { get; set; }
        public List<FareDetailsBySegment> FareDetailsBySegment { get; set; }
    }

    public class FareDetailsBySegment
    {
        public string SegmentId { get; set; }
        public string Cabin { get; set; }
        public string FareBasis { get; set; }
        public string Class { get; set; }
        public IncludedCheckedBags IncludedCheckedBags { get; set; }
    }

    public class IncludedCheckedBags
    {
        public int Weight { get; set; }
        public string WeightUnit { get; set; }
    }


}