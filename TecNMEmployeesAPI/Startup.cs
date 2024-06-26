using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using TecNMEmployeesAPI.Services;

namespace TecNMEmployeesAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(typeof(Startup));

            // Subir archivos local wwwroot
            services.AddTransient<IFileStorage, LocalFileStorage>();
            services.AddHttpContextAccessor();

            // Obtener las variables de entorno
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? Configuration["DB_SERVER"];
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? Configuration["DB_PORT"];
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? Configuration["DB_USER"];
            var dbPwd = Environment.GetEnvironmentVariable("DB_PWD") ?? Configuration["DB_PWD"];

            // Construir la cadena de conexión
            var connectionString = $"Server={dbServer};Port={dbPort};User Id={dbUser};Password={dbPwd};";

            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Identity 
            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Configuration["JWT:Seed"])),
                        ClockSkew = TimeSpan.Zero
                    });

            // Autorización basada en Claims
            services.AddAuthorization(options =>
            {
                options.AddPolicy("EmployeesRead", policy => policy.RequireClaim("Employees", "Read"));
                options.AddPolicy("EmployeesCreateOrUpdate", policy => policy.RequireClaim("Employees", "Create", "Update"));
                options.AddPolicy("USER", policy => policy.RequireClaim("isUser"));
            });

            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            services.AddControllers()
                    .AddJsonOptions(options =>
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
                    .AddNewtonsoftJson();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TecNMEmployeesAPI", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TecNMEmployeesAPI v1"));

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
