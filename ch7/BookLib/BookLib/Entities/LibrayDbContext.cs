using BookLib.Extentions;
using Microsoft.EntityFrameworkCore;

namespace BookLib.Entities
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //利用ModelBuilder的扩展方法SeedData创建测试数据
            modelBuilder.SeedData();
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}
