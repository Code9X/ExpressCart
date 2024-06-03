using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
	{
        private ApplicationDbContext _db;
        public ProductImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
            
        public void Update(ProductImage obj)
        {
            _db.ProductImages.Update(obj);
        }
    }
}
