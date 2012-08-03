using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace otr_project.Models
{
    public class DBInit : DropCreateDatabaseIfModelChanges<MarketPlaceEntities>
    {
        protected override void Seed(MarketPlaceEntities context)
        {
            SeedAddressData(context);
            //SeedTestData(context);

            var categories = new List<Category>
            {
                new Category { Name = "Tools" },
                new Category { Name = "Home" },
                new Category { Name = "Luxury" },
                new Category { Name = "Automobile" },
                new Category { Name = "Garden" },
                new Category { Name = "Sports" },
                new Category { Name = "Outdoor" },
            };

            foreach (Category c in categories)
            {
                context.Categories.Add(c);
            }

            var badges = new List<Badge>
            {
                new Badge { Name = "Individual" },
                new Badge { Name = "Business" }
            };

            foreach (Badge b in badges)
            {
                context.Badges.Add(b);
            }

        }

        private void SeedAddressData(MarketPlaceEntities context)
        {
            var countries = new List<Country>
            {
                new Country { Name = "Canada" }
            };

            foreach (Country c in countries)
            {
                context.Countries.Add(c);
            }

            var regions = new List<Region>
            {
                new Region { Name = "Quebec", CountryId = 1},
                new Region { Name = "Alberta", CountryId = 1},
                new Region { Name = "Ontario", CountryId = 1},
                new Region { Name = "Manitoba", CountryId = 1},
                new Region { Name = "Nova Scotia", CountryId = 1},
                new Region { Name = "Saskatchewan", CountryId = 1},
                new Region { Name = "Newfoundland and Labrador", CountryId = 1},
                new Region { Name = "New Brunswick", CountryId = 1},
                new Region { Name = "British Columbia", CountryId = 1},
                new Region { Name = "Prince Edward Island", CountryId = 1},
                new Region { Name = "Northwest Territories", CountryId = 1},
                new Region { Name = "Yukon Territory", CountryId = 1},
                new Region { Name = "Nunavut", CountryId = 1}    
            };

            foreach (Region r in regions)
            {
                context.Regions.Add(r);
            }
        }

        private void SeedTestData(MarketPlaceEntities context)
        {
            context.Users.Add(new UserModel
            {
                FirstName = "Subu",
                LastName = "Ram",
                Email = "i@i.com",
                isFacebookUser = false,
                RegionId = 1,
                City = "Mississauga"
            });

            for (int i = 0; i < 100; i++)
            {
                ItemModel item = new ItemModel
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Name = "Test Item " + i,
                    Desc = "Test Item " + i + " Description.",
                    CategoryId = 100 % (i+1),
                    UserModelEmail = "i@i.com",
                    CostPerDay = decimal.Parse(i.ToString()),
                    SecurityDeposit = decimal.Parse((100 + i).ToString()),
                    ImageCount = 0,
                    DateCreated = System.DateTime.Now,
                    isActive = true
                };
                
                context.Items.Add(item);
            }
        }
    }
}