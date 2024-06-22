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
        string GetCountryApiUrl();
        string GetCountryApiKey();
        string GetFlightApiUrl();
        string GetFlightApiKey();
    }
}
