//using Microsoft.EntityFrameworkCore;

//namespace LC.ApiKey.Models;

//public class AppDbContext : DbContext
//{
//    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//    //public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
//    public DbSet<ApiKey> ApiKeys { get; set; }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        // Seed a test user
//        //modelBuilder.Entity<User>().HasData(
//        //    new User
//        //    {
//        //        Id = 1,
//        //        Username = "testuser",
//        //        Password = "testpassword"
//        //    }
//        //);
//    }
//}