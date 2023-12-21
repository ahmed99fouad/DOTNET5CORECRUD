using Microsoft.EntityFrameworkCore;



namespace DOTNET5CORECRUD.Models
{
    public class ApplicatioDBContext :DbContext
    {
        public ApplicatioDBContext(DbContextOptions<ApplicatioDBContext>options): base(options) 
             { 

              }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Movie> Movies { get; set; }

    }
}
