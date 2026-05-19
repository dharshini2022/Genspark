//Creational Pattern : Builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//AddController() registers all the controllers in routing table
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//Swagger based checking
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//organizes all the objects in the Builder to build the final product

var app = builder.Build();

//Enviroment : Development (work and testing of code)
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//Frontend request -> Routing Table -> Exact Controller -> Exact Method
app.MapControllers();
//starts the web server
app.Run();
