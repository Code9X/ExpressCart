using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository
{
    public class SubCategoryRepository : Repository<SubCategory>, ISubCategoryRepository
    {
        private ApplicationDbContext _db;
        public SubCategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
            
        public void Update(SubCategory obj)
        {
            _db.SubCategories.Update(obj);
        }
    }
}
