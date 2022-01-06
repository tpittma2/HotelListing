using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Country>().HasData(
                new Country
                {
                    Id=1,
                    Name = "United States of America",
                    ShortName = "US",
                },
                new Country
                {
                    Id = 2,
                    Name = "Canada",
                    ShortName = "CA",
                },
                new Country
                {
                    Id = 3,
                    Name = "Mexico",
                    ShortName = "MX",
                }
                );

            builder.Entity<Hotel>().HasData(
                new Hotel
                {
                    Id = 1,
                    Name = "Hilton Hotel",
                    Address = "Georgia",
                    CountryId = 1,
                    Rating = 4.5,
                },
                new Hotel
                {
                    Id = 2,
                    Name = "Sandal Resort and Spa",
                    Address = "Ontario",
                    CountryId = 2,
                    Rating = 4.0,
                },
                new Hotel
                {
                    Id = 3,
                    Name = "Comfort Suites",
                    Address = "Mexico City",
                    CountryId = 1,
                    Rating = 4.3,
                }
                ); ;
        }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Hotel> Hotels { get; set; }

        
    }
}
