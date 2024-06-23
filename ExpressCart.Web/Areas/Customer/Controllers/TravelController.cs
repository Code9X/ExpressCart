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
            var depLoc = travelvm.Travel.DepartureLocation.ToString();
            var destLoc = travelvm.Travel.DestinationLocation.ToString();
            var depDate = travelvm.Travel.DepartureDate.ToString("yyyy-MM-dd");
            var destDate = travelvm.Travel.DestinationDate.ToString("yyyy-MM-dd");
            int adults = travelvm.Travel.Adults_count;
            int childrens = travelvm.Travel.Childerns_count;
            double maxPrice = travelvm.MaxPrice;
            int maxCount = travelvm.MaxCount;
            var currCode = travelvm.CurrencyCode.ToString();
            var travelClass = travelvm.Class.ToString();
            bool nonStop = travelvm.NonStop;

            var root = await GetFlightDetailsAsync(depLoc, destLoc, depDate, destDate, adults, childrens, travelClass, nonStop, currCode, maxPrice, maxCount);

            if (root.Dictionaries?.Carriers != null && root.Dictionaries?.Aircraft != null)
            {
                // Inject carrier and aircraft names into flight details
                foreach (var flight in root.data)
                {
                    foreach (var itinerary in flight.Itineraries)
                    {
                        foreach (var segment in itinerary.Segments)
                        {
                            if (root.Dictionaries.Carriers.ContainsKey(segment.CarrierCode))
                            {
                                flight.CarrierName = root.Dictionaries.Carriers[segment.CarrierCode];
                            }
                            if (root.Dictionaries.Aircraft.ContainsKey(segment.Aircraft.Code))
                            {
                                flight.AircraftName = root.Dictionaries.Aircraft[segment.Aircraft.Code];
                            }
                        }
                    }
                }
            }

            travelvm.FlightDetails = root.data;

            HttpContext.Session.SetString("travelvm", JsonConvert.SerializeObject(travelvm));

            return RedirectToAction("FlightOverview");
        }

        public IActionResult FlightOverview()
        {
            var travelvmJson = HttpContext.Session.GetString("travelvm");
            if (string.IsNullOrEmpty(travelvmJson))
            {
                return RedirectToAction("Index");
            }

            var travelvm = JsonConvert.DeserializeObject<TravelVM>(travelvmJson);

            return View(travelvm);
        }


        private async Task<List<AirportDetails>> GetAirportDtlsAsync()
        {
            return await Task.FromResult(new List<AirportDetails>
            {
                new AirportDetails { Code = "COK", Name = "Cochin International Airport" },
                new AirportDetails { Code = "BKK", Name = "Bangkok International Airport" },
                new AirportDetails { Code = "DXB", Name = "Dubai International Airport" },
                new AirportDetails { Code = "JFK", Name = "John F. Kennedy International Airport" },
                new AirportDetails { Code = "LAX", Name = "Los Angeles International Airport" },
                new AirportDetails { Code = "ORD", Name = "O'Hare International Airport" }
            });
        }

        private async Task<Root> GetFlightDetailsAsync(string depLoc, string destLoc, string depDate, string destDate, int adults, int children, string travelClass, bool nonStop, string currCode, double maxPrice, int maxCount)
        {
            string baseUrl = _apiRepository.GetFlightApiUrl();
            string accessToken = await GetAccessTokenAsync();
            string formattedUrl = GetFormattedURL(baseUrl, depLoc, destLoc, depDate, destDate, adults, children, travelClass, nonStop, currCode, maxPrice, maxCount);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(formattedUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var root = JsonConvert.DeserializeObject<Root>(json);

                    return root;
                }
                else
                {
                    string message = response.StatusCode.ToString();
                    TempData["success"] = message;
                }

                return new Root();
            }
        }


        private async Task<string> GetAccessTokenAsync()
        {
            string clientId = _apiRepository.GetFlightApiKey();
            string clientSecret = _apiRepository.GetFlightAPISecret();
            string tokenUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });

                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                    return tokenResponse.AccessToken;
                }
                else
                {
                    throw new Exception("Unable to retrieve access token.");
                }
            }
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }

        private string GetFormattedURL(string baseUrl, string depLoc, string destLoc, string depDate, string destDate, int adults, int children, string travelClass, bool nonStop, string currCode, double maxPrice, int maxCount)
        {
            try
            {
                string formattedUrl = string.Empty;
                if (destDate == "0001-01-01") // If One Way is Selected
                {
                    string adjustedBaseUrl = baseUrl.Replace("&children={5}", "");

                    formattedUrl = string.Format(adjustedBaseUrl, depLoc, destLoc, depDate, "", adults, children, travelClass, nonStop.ToString().ToLower(), currCode, maxPrice, maxCount);
                    formattedUrl = formattedUrl.Replace("&returnDate=", "");
                }
                else // Round Trip
                {
                    string adjustedBaseUrl = baseUrl.Replace("&children={5}", "");

                    formattedUrl = string.Format(adjustedBaseUrl, depLoc, destLoc, depDate, destDate, adults, children, travelClass, nonStop.ToString().ToLower(), currCode, maxPrice, maxCount);
                }
                return formattedUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("Error formatting URL", ex);
            }
        }




    }
}