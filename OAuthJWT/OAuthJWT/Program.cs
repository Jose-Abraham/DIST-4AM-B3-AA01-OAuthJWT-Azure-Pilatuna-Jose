using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OAuthJWT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(); // <-- Agrega Swagger UI

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(); // <-- Habilita la interfaz visual en /swagger
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}