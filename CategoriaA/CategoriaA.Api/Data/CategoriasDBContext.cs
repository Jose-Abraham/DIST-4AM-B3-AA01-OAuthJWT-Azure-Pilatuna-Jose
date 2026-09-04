using Microsoft.EntityFrameworkCore;
using CategoriaA.Api.Models;

namespace CategoriaA.Api.Data
{
    public class CategoriasDBContext : DbContext
    {
        public CategoriasDBContext(DbContextOptions<CategoriasDBContext> options) : base(options)
        {
        }

        public DbSet<Categorias> Categorias { get; set; }
    }
}
