using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository.IRepository
{
    public interface IAPIRepository
    {
        string GetRazorSecretKey();
        string GetRazorPublishableKey();
        string GetCityApiUrl();
        string GetFlightApiUrl();
        string GetFlightApiKey();
        string GetFlightAPISecret();
    }
}
