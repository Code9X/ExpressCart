using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository
{
    public class AddvertisementImageRepository : Repository<AddvertisementImage>, IAddvertisementImageRepository
	{
        private ApplicationDbContext _db;
        public AddvertisementImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
            
        public void Update(AddvertisementImage obj)
        {
            _db.AddvertisementImages.Update(obj);
        }
    }
}
