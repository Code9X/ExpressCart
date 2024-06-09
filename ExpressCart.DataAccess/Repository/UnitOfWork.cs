using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public ISubCategoryRepository SubCategory { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IProductRepository Product { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IAddvertisementRepository Addvertisement { get; private set; }
        public IAddvertisementImageRepository AddvertisementImage { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
		public IOrderHeaderRepository OrderHeader { get; private set; }
		public IOrderDetailRepository OrderDetail { get; private set; }
		public ITravelRepository Travel { get; private set; }

		public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            SubCategory = new SubCategoryRepository(_db);
            Company = new CompanyRepository(_db);
			Product = new ProductRepository(_db);
			ProductImage = new ProductImageRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
			Addvertisement = new AddvertisementRepository(_db);
			AddvertisementImage = new AddvertisementImageRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
			OrderHeader = new OrderHeaderRepository(_db);
			OrderDetail = new OrderDetailRepository(_db);
            Travel = new TravelRepository(_db);
		}
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
