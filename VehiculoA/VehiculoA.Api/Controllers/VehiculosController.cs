using VehiculoA.Api.Data;
using VehiculoA.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VehiculoA.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculosController : ControllerBase
    {
        private readonly VehiculosDBContext _dbcontext;
     

        public VehiculosController(VehiculosDBContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehiculos>>> GetVehiculos()
        {
            var vehiculos = await _dbcontext.Vehiculos.AsNoTracking().ToListAsync();
            return Ok(vehiculos);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehiculos>> GetVehiculo(int id)
        {
            var vehiculo = await _dbcontext.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.IdVehiculo == id);

            if(vehiculo == null) return NotFound();
            return Ok(vehiculo);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult<Vehiculos>> CrearVehiculo(Vehiculos vehiculo)
        {
            _dbcontext.Vehiculos.Add(vehiculo);
            await _dbcontext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.IdVehiculo }, vehiculo);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Vehiculos>> ActualizarVehiculo(int id, Vehiculos vehiculo)
        {
            if(id != vehiculo.IdVehiculo) return BadRequest();

            _dbcontext.Entry(vehiculo).State = EntityState.Modified;

            await _dbcontext.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarVehiculo(int id)
        {
            var vehiculo = await _dbcontext.Vehiculos.FindAsync(id);
            if (vehiculo == null) return NotFound();
            _dbcontext.Vehiculos.Remove(vehiculo);
            await _dbcontext.SaveChangesAsync();
            return NoContent();
        }
    }
}
