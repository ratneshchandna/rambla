using System.Data.Entity;
using System.Data.Entity.Design;

namespace otr_project.Models
{
    public class MarketPlaceEntities : DbContext
    {
        public DbSet<ItemModel> Items { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<BlackoutDate> BlackoutDates { get; set; }
        public DbSet<CartItemModel> CartItems { get; set; }
        public DbSet<OrderDetailModel> OrderDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<ThreadModel> Threads { get; set; }
        public DbSet<MessageModel> Messages { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<FacebookUser> FacebookUsers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        //public DbSet<City> Cities { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<UserImageFileModel> Files { get; set; }
        public DbSet<ItemImageFileModel> ItemImages { get; set; }


        protected override void OnModelCreating(DbModelBuilder builder)
        {
            // At some point it started complaining about the user's relationship and not just the item's relation to Region and Country
            // So I had to do this and it fixed those exceptions
            builder.Entity<ItemModel>().HasRequired(i => i.ItemRegion).WithMany().HasForeignKey(i=>i.RegionId).WillCascadeOnDelete(false);
            builder.Entity<ItemModel>().HasRequired(i => i.ItemCountry).WithMany().HasForeignKey(i => i.CountryId).WillCascadeOnDelete(false);
            builder.Entity<ItemModel>().HasMany(i => i.BlackoutDates).WithRequired().HasForeignKey(i => i.ItemModelId).WillCascadeOnDelete(true);
            builder.Entity<ItemModel>().HasMany(i => i.ItemImages).WithRequired().HasForeignKey(i => i.ItemModelId).WillCascadeOnDelete(true);
            builder.Entity<UserModel>().HasRequired(i => i.UserRegion).WithMany().HasForeignKey(i => i.RegionId).WillCascadeOnDelete(false);
            builder.Entity<UserModel>().HasRequired(i => i.UserCountry).WithMany().HasForeignKey(i => i.CountryId).WillCascadeOnDelete(false);

            base.OnModelCreating(builder);
        }

        //Helper methods
        public bool DeleteItem(ItemModel item)
        {
            if (item == null)
                return false;

            Items.Remove(item);

            return true; 
        }

        public ItemModel GetItem(string id)
        {
            if (id == null)
                return null;

            return Items.Find(id);
        }

        public int AddItemToUser(ItemModel item, string uName)
        {
            var user = Users.Find(uName);

            if (user != null)
            {
                user.Items.Add(item);
                return user.Items.Count;
            }

            return -1;
        }

        public bool IsUserAuthorized(ItemModel item, string uName)
        {
            var users = Users.Find(uName);

            if (users != null)
            {
                foreach (ItemModel i in users.Items)
                {
                    if (i.Id == item.Id)
                        return true;
                }
            }

            return false;
        }
    }
}