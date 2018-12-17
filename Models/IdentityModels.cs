using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace BonTemps.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        //NAW gegevens
        public string Naam { get; set; }
        public string Adres { get; set; }
        public string Woonplaats { get; set; }

        public virtual ICollection<Reservering> Reserveringen { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<BonTemps.Models.Gerecht> Gerechts { get; set; }

        public System.Data.Entity.DbSet<BonTemps.Models.Menu> Menus { get; set; }

        public System.Data.Entity.DbSet<BonTemps.Models.Reservering> Reserverings { get; set; }

        public DbSet<BonTemps.Models.Couverts> Couverts { get; set; }

        public System.Data.Entity.DbSet<BonTemps.Models.Ingredient> Ingredients { get; set; }

        public DbSet<TotalIngredients> TotalIngredients { get; set; }
    }
}