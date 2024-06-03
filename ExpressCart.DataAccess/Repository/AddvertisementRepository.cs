using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository
{
    public class AddvertisementRepository : Repository<Addvertisement>, IAddvertisementRepository
	{
        private ApplicationDbContext _db;
        public AddvertisementRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
            
        public void Update(Addvertisement obj)
        {
            _db.Addvertisements.Update(obj);
        }
    }
}
