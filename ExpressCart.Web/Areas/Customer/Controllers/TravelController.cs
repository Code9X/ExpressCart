using System.Drawing.Text;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;
using ExpressCart.Models.ViewModels;
using ExpressCart.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Razorpay.Api;
using Stripe;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ExpressCartWeb.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class TravelController : Controller //Here i have ignored the SSL Certificate for Testing, change that in Production
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
        [HttpPost]
        public IActionResult FlightDetails(string flightId)
        {
            var travelvmJson = HttpContext.Session.GetString("travelvm");
            if (string.IsNullOrEmpty(travelvmJson))
            {
                // Handle the case where the session data is not available
                return RedirectToAction("Index");
            }

            var travelvm = JsonConvert.DeserializeObject<TravelVM>(travelvmJson);
            var flightDetail = travelvm.FlightDetails.FirstOrDefault(f => f.Id == flightId);

            if (flightDetail == null)
            {
                // Handle the case where the flight detail is not found
                return RedirectToAction("FlightOverview");
            }

            travelvm.SelectedFlight = flightDetail;
            TempData["flightId"] = flightId;

            return View(travelvm);
        }
        [HttpPost]
        public IActionResult FlightPayment() //Here Total * 100 is been given, and then limit exceeded in the razor Pay, so i have changed * 100
        {
            var flightId = TempData["flightId"] as string;
            var travelvmJson = HttpContext.Session.GetString("travelvm");
            var travelvm = JsonConvert.DeserializeObject<TravelVM>(travelvmJson);
            var flightDetail = travelvm.FlightDetails.FirstOrDefault(f => f.Id == flightId);

            var travelData = new Travel
            {
                UserId = GetUserId(),
                DepLoc = flightDetail.Itineraries[0].Segments[0].Departure.IataCode,
                DepTerm = flightDetail.Itineraries[0].Segments[0].Departure.Terminal,
                DepDate = flightDetail.Itineraries[0].Segments[0].Departure.At.ToString(),
                Stops = flightDetail.Itineraries[0].Segments.Count,
                DestLoc = flightDetail.Itineraries[0].Segments.Max(s => s.Arrival.IataCode),
                DestTerm = flightDetail.Itineraries[0].Segments.Max(s => s.Arrival.Terminal),
                DestDate = flightDetail.Itineraries[0].Segments.Max(s => s.Arrival.At.ToString()),
                Adults = travelvm.Travel.Adults,
                Childerns = travelvm.Travel.Childerns,
                CarrierName = flightDetail.CarrierName,
                AircraftName = flightDetail.AircraftName,
                Code = flightDetail.Itineraries[0].Segments[0].Aircraft.Code,
                OneWay = flightDetail.OneWay,
                BasePrice = flightDetail.Price.Currency + " " + flightDetail.Price.Base,
                TotalPrice = flightDetail.Price.Currency + " " + flightDetail.Price.GrandTotal
            };

            _unitOfWork.Travel.Add(travelData);
            _unitOfWork.Save();

            // Generate Razorpay order
            string secretKey = _apiRepository.GetRazorSecretKey();
            string publishableKey = _apiRepository.GetRazorPublishableKey();

            decimal grandTotalDecimal = decimal.Parse(flightDetail.Price.GrandTotal);
            int GrandTotal = (int)Math.Round(grandTotalDecimal);

            RazorpayClient client = new RazorpayClient(secretKey, publishableKey);
            Dictionary<string, object> options = new Dictionary<string, object>();  
            options.Add("amount", GrandTotal);
            options.Add("receipt", "order_rcptid_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            options.Add("currency", flightDetail.Price.Currency);
            options.Add("payment_capture", "1"); // 1 - automatic  , 0 - manual

            Order orderResponse = client.Order.Create(options);
            string razorOrderId = orderResponse["id"].ToString();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
            var name = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var StreetAddress = claimsIdentity.FindFirst(ClaimTypes.StreetAddress)?.Value;

            // Create RazorOrder model for return to the view
            RazorOrder razorOrder = new RazorOrder
            {
                orderId = razorOrderId,
                razorpayKey = secretKey,
                amount = GrandTotal,
                currency = flightDetail.Price.Currency,
                name = name,
                email = email,
                //contactNumber = "99993449343",
                address = StreetAddress,
                description = "Test Mode"
            };

            TempData["RazorOrder"] = JsonConvert.SerializeObject(razorOrder);

            return View("FlightPayment", Tuple.Create(razorOrder, travelData.Id));
        }
        public IActionResult OrderConfirmation(int Id, string paymentStatus, string paymentId)
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAirportDetails(string keyword)
        {
            var airportDetails = await GetAirportDtlsAsync(keyword);
            return Json(airportDetails);
        }
        private async Task<List<CityAndAirport>> GetAirportDtlsAsync(string keyword)
        {
            string baseUrl = _apiRepository.GetCityApiUrl();
            string accessToken = await GetAccessTokenAsync();
            string formattedUrl = GetFormattedURL(baseUrl, Keyword: keyword);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; // Ignore SSL certificate errors for testing purposes

                using (var client = new HttpClient(handler))
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
                                    Name = item.address.cityName,
                                    DetailedName = item.detailedName
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
        }
        private async Task<Root> GetFlightDetailsAsync(TravelVM travelvm)
        {
            string baseUrl = _apiRepository.GetFlightApiUrl();
            string accessToken = await GetAccessTokenAsync();

            string formattedUrl = GetFormattedURL(baseUrl, Travelvm:travelvm);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; // Ignore SSL certificate errors for testing purposes

                using (var client = new HttpClient(handler))
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
        }
        private async Task<string> GetAccessTokenAsync()
        {
            string clientId = _apiRepository.GetFlightApiKey();
            string clientSecret = _apiRepository.GetFlightAPISecret();
            string tokenUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";

            using (var handler = new HttpClientHandler())
            {
                // Ignore SSL certificate errors for testing purposes
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    // Add headers that Postman might be including
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("PostmanRuntime/7.26.8");

                    var content = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(tokenUrl, content);
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                            return tokenResponse.AccessToken;
                        }
                        else
                        {
                            throw new Exception($"Unable to retrieve access token. Response: {responseContent}");
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        throw new Exception($"Request exception: {e.Message}", e);
                    }
                }
            }
        }
        private string GetFormattedURL(string baseUrl, TravelVM Travelvm = null, string Keyword = null)
        {
            if (Travelvm != null)
            {
                var depLoc = Travelvm.Travel.DepLoc.ToString();
                var destLoc = Travelvm.Travel.DestLoc.ToString();
                var depDate = Travelvm.Travel.DepDate.ToString();
                var destDate = Travelvm.Travel.DestDate?.ToString() ?? string.Empty;
                int adults = Travelvm.Travel.Adults;
                int children = Travelvm.Travel.Childerns;
                double maxPrice = Travelvm.MaxPrice;
                int maxCount = Travelvm.MaxCount;
                var currCode = Travelvm.CurrencyCode.ToString();
                var travelClass = Travelvm.Class.ToString();
                bool nonStop = Travelvm.NonStop;

                try
                {
                    string formattedUrl = string.Empty;
                    if (destDate == "") // If One Way is Selected
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