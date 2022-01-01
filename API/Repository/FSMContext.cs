using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class FSMContext : DbContext
    {
        public FSMContext(DbContextOptions<FSMContext> options) : base(options) { }
        public DbSet<Organisation> organisation { get; set; }
        public DbSet<Player> player { get; set; }
        public DbSet<Scrim> scrim { get; set; }
        public DbSet<Team> team { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("FSMdb");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
