using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository.IRepository
{
    public interface IAddvertisementImageRepository : IRepository<AddvertisementImage>
    {
        void Update(AddvertisementImage obj);
    }
}
