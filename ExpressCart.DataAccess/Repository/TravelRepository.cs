using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository
{
    public class TravelRepository : Repository<Travel>, ITravelRepository
	{
        private ApplicationDbContext _db;
        public TravelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
            
        public void Update(Travel obj)
        {
            _db.Travels.Update(obj);
        }
    }
}
