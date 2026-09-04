using VehiculoA.Api.Data;
using VehiculoA.Api.Events;
using VehiculoA.Api.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace VehiculoA.Api.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQConsumer> _logger;

        private readonly IServiceScopeFactory _scopeFactory;



        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQConsumer(
            IConfiguration configuration,
            ILogger<RabbitMQConsumer> logger,
            IServiceScopeFactory scopeFactory
            )
        {
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            var queueName = _configuration["RabbitMQ:QueueName"]!;

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();

                var mensaje = Encoding.UTF8.GetString(body);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var evento = JsonSerializer.Deserialize<CategoriaCreadoEvento>(mensaje, options);

                if (evento != null)
                {
                    _logger.LogInformation(
                        "Categoria creada recibida. IdCategoria: {IdCategoria}",
                        evento.idCategoria
                    );

                    using var scope = _scopeFactory.CreateScope();

                    var dbContext =
                        scope.ServiceProvider
                            .GetRequiredService<VehiculosDBContext>();

                    var existe = await dbContext.Vehiculos
                        .AnyAsync(i => i.IdCategoria == evento.idCategoria);

                    if (!existe)
                    {
                        var vehiculo = new Vehiculos
                        {
                            IdCategoria = evento.idCategoria,
                            Marca = "Sin asignar",
                            Modelo = "Sin asignar",
                            Precio = 0,
                            Stock = 0,
                            Estado = true

                        };

                        dbContext.Vehiculos.Add(vehiculo);

                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation(
                            "Vehículo creado automáticamente para IdCategoria: {IdCategoria}",
                            evento.idCategoria
                        );
                    }
                }

                await _channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false
                );
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );

            await Task.Delay(
                Timeout.Infinite,
                stoppingToken
            );
        }
    }
}
