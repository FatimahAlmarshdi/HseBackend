
using Microsoft.EntityFrameworkCore;
using HseBackend.Models;

namespace HseBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Report> Reports { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<InspectionQuestion> InspectionQuestions { get; set; }
        public DbSet<InspectionResponse> InspectionResponses { get; set; }
    }
}
