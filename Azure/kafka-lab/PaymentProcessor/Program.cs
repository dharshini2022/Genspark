using Confluent.Kafka;
 
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
 
var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
builder.Services.AddSingleton<IProducer<string, string>>(
    new ProducerBuilder<string, string>(producerConfig).Build());
 
var app = builder.Build();
app.MapControllers();
app.Run("http://localhost:5100");
