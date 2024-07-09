using System.Threading;
using Newtonsoft.Json;

namespace ExpressCart.Models.ViewModels
{
    public class TravelVM   
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string RandomAirportCode { get; set; }
        public string RandomAirportName { get; set; }
        public Travel Travel { get; set; }
        public string Class { get; set; }
        public string CurrencyCode { get; set; }
        public double MaxPrice { get; set; }
        public bool NonStop { get; set; }
        public int MaxCount { get; set; }
        public int Type { get; set; }
        public List<FlightData> FlightDetails { get; set; }
        public FlightData SelectedFlight { get; set; }
    }

    public class Root
    {
        public List<FlightData> data { get; set; }
        public Dictionaries Dictionaries { get; set; }
    }

    #region data
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
        public string CarrierName { get; set; } // Added property
        public string AircraftName { get; set; }
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
        public string Terminal { get; set; }
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
        public double Total { get; set; }
        public string Base { get; set; }
        public List<Fee> Fees { get; set; }
        public double GrandTotal { get; set; }
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
    #endregion

    #region dictionaries
    public class Dictionaries
    {
        public Dictionary<string, Location> Locations { get; set; }
        public Dictionary<string, string> Aircraft { get; set; }
        public Dictionary<string, string> Currencies { get; set; }
        public Dictionary<string, string> Carriers { get; set; }
    }

    public class Location
    {
        public string CityCode { get; set; }
        public string CountryCode { get; set; }
    }
    #endregion

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

}
