using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCart.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
		ICategoryRepository Category { get; }
        ISubCategoryRepository SubCategory { get; }
        ICompanyRepository Company { get; }
		IProductRepository Product { get; }
		IProductImageRepository ProductImage { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IAddvertisementRepository Addvertisement { get; }
        IAddvertisementImageRepository AddvertisementImage { get; }
        IApplicationUserRepository ApplicationUser { get; }
		IOrderHeaderRepository OrderHeader { get; }
		IOrderDetailRepository OrderDetail { get; }
		ITravelRepository Travel { get; }
		void Save();
    }
}