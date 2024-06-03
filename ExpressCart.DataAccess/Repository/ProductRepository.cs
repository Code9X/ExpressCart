using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
	{
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

		public void Update(Product obj)
		{
			var objFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
			if (objFromDb != null)
			{
				objFromDb.Name = obj.Name;
				objFromDb.CategoryID = obj.CategoryID;
				objFromDb.Price = obj.Price;
				objFromDb.OfferPrice = obj.OfferPrice;
				objFromDb.SubCategoryID = obj.SubCategoryID;
				objFromDb.CompanyID = obj.CompanyID;
				objFromDb.DisplayOrder = obj.DisplayOrder;
				objFromDb.ProductImages = obj.ProductImages;
				objFromDb.Rating = obj.Rating;
				objFromDb.Specifications = obj.Specifications;
			}
		}
	}
}
