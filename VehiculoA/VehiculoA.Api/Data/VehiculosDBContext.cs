using Microsoft.EntityFrameworkCore;
using VehiculoA.Api.Models;
namespace VehiculoA.Api.Data
{
    public class VehiculosDBContext : DbContext
    {
        public VehiculosDBContext(DbContextOptions<VehiculosDBContext> options) : base(options)
        {
        }

        public DbSet<Vehiculos> Vehiculos { get; set; }
    }
}
