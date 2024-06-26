using TecNMEmployeesAPI;

var builder = WebApplication.CreateBuilder(args);

// Añadir soporte para variables de entorno
builder.Configuration.AddEnvironmentVariables();

// Si quieres configurar Kestrel para escuchar en un puerto específico desde variables de entorno
var port = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "80";
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port)); // Escucha en el puerto especificado en todas las direcciones IP
});

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

app.Run();
