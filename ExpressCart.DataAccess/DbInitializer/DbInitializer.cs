using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressCart.DataAccess.Data;
using ExpressCart.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExpressCart.DataAccess.DbInitializer;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.DbInitializer
{
	public class DbInitializer : IDbInitializer
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _db;

		public DbInitializer(
			UserManager<IdentityUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ApplicationDbContext db)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_db = db;
		}
		public void Initialize()
		{
			//Migration if they are not applied
			try
			{
				if(_db.Database.GetPendingMigrations().Count()>0)
				{
					_db.Database.Migrate();
				}
			}
			catch (Exception)
			{
				throw;
			}

			//Create roles if they are not created 
			if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
			{
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

				//If roles are not created, then we will create admin user as well
				_userManager.CreateAsync(new ApplicationUser
				{
					UserName = "admin@ec.com",
					Email = "admin@ec.com",
					Name = "ADMIN",
					PhoneNumber = "99999999",
					StreetAddress = "147 Avenue Street",
					City = "Kochi",
					State = "Kerala",
					PostalCode = "75156"
				},"Admin123#").GetAwaiter().GetResult();

				ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u=>u.Email == "admin@ec.com");
				_userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
			}
			return;
		}
	}
}
