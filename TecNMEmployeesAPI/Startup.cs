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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            // AutoMapper
            services.AddAutoMapper(typeof(Startup));


            // Subir archivos local wwwroot
            services.AddTransient<IFileStorage, LocalFileStorage>();
            // Servicios de context accessor que utilizamos en el LocalFileStorage
            services.AddHttpContextAccessor();


            // Obtener las variables de entorno
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? Configuration["DB_SERVER"];
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? Configuration["DB_PORT"];
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? Configuration["DB_USER"];
            var dbPwd = Environment.GetEnvironmentVariable("DB_PWD") ?? Configuration["DB_PWD"];


            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"))));

           

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
                        }
                    );

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TecNMEmployeesAPI v1"));
            //}

            app.UseHttpsRedirection();

            // Nuestro proyecto sirva contenido statico
            // Lo configure para que podamos ver las imagenes en nuestro wwwroot
            // Ejemplo: https://localhost:7039/actors/d9c22d95-f852-4e93-a264-f31d7f7a00f7.jpg
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
