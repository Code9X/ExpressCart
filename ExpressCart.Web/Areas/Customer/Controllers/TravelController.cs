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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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

            var travelvm = new TravelVM
            {
                CategoryId = categoryId,
                CategoryName = category.Name,
                Travel = new Travel()
            };

            return View(travelvm);
        }
        [HttpPost]
        public async Task<IActionResult> Index(TravelVM travelvm)
        {
            var root = await GetFlightDetailsAsync(travelvm);

            if (root.Dictionaries?.Carriers != null && root.Dictionaries?.Aircraft != null)
            {
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
        [HttpGet]
        public async Task<IActionResult> GetAirportDetails(string keyword)
        {
            var airportDetails = await GetAirportDtlsAsync(keyword);
            return Json(airportDetails);
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
        private async Task<List<CityAndAirport>> GetAirportDtlsAsync(string keyword)
        {
            string baseUrl = _apiRepository.GetCityApiUrl();
            string accessToken = await GetAccessTokenAsync();
            string formattedUrl = GetFormattedURL(baseUrl, Keyword: keyword);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(formattedUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(json);

                    var cityAndAirports = new List<CityAndAirport>();

                    if (result.data != null)
                    {
                        foreach (var item in result.data)
                        {
                            cityAndAirports.Add(new CityAndAirport
                            {
                                Code = item.iataCode,
                                Name = item.address.cityName
                            });
                        }
                    }
                    else
                    {
                        TempData["success"] = "No matching locations found.";
                    }

                    return cityAndAirports;
                }
                else
                {
                    string message = response.StatusCode.ToString();
                    TempData["success"] = message;
                }

                return new List<CityAndAirport>();
            }
        }
        private async Task<Root> GetFlightDetailsAsync(TravelVM travelvm)
        {
            string baseUrl = _apiRepository.GetFlightApiUrl();
            string accessToken = await GetAccessTokenAsync();

            string formattedUrl = GetFormattedURL(baseUrl, Travelvm:travelvm);

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
        private string GetFormattedURL(string baseUrl, TravelVM Travelvm = null, string Keyword = null)
        {
            if (Travelvm != null)
            {
                var depLoc = Travelvm.Travel.DepartureLocation.ToString();
                var destLoc = Travelvm.Travel.DestinationLocation.ToString();
                var depDate = Travelvm.Travel.DepartureDate.ToString("yyyy-MM-dd");
                var destDate = Travelvm.Travel.DestinationDate.ToString("yyyy-MM-dd");
                int adults = Travelvm.Travel.Adults_count;
                int children = Travelvm.Travel.Childerns_count;
                double maxPrice = Travelvm.MaxPrice;
                int maxCount = Travelvm.MaxCount;
                var currCode = Travelvm.CurrencyCode.ToString();
                var travelClass = Travelvm.Class.ToString();
                bool nonStop = Travelvm.NonStop;

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
            else if (!string.IsNullOrEmpty(Keyword))
            {
                try
                {
                    return string.Format(baseUrl, Keyword);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error formatting URL with keyword", ex);
                }
            }
            else
            {
                throw new ArgumentException("Either travelvm or Keyword must be provided");
            }
        }


    }
}