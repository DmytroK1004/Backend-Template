using Application.Queries;
using FluentValidation;
using Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using Infrastructure.Interfaces.Repositories;
using Application.Interfaces;
using Application.Queries.Users.GetAllUsers;
using MediatR;
using Application.Commands.Users.UpdateUser;
using Application.Commands.Users.CreateUser;
using Application.Commands.Users.DeleteUser;
using Application.Commands.Users.SignupUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Commands.Users.SigninUser;
using Presentation.Exceptions;
using Presentation.Middleware;

namespace Presentation
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend");
                c.DocExpansion(DocExpansion.None);
                c.DisplayOperationId();
                c.DisplayRequestDuration();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("_allowSpecificOrigins");

            app.UseMiddleware<CustomErrorMiddleware>(app.ApplicationServices.GetService<ILogger<IBestPracticesException>>());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Key"]);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
            });

            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("_allowSpecificOrigins", policyBuilder =>
                {
                    policyBuilder
                        .WithOrigins(Configuration["CorsUrl"])
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });



            // service
            services.AddSingleton<TokenService>();

            // dbContext
            services.AddDbContext<DBContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // data
            services.AddScoped<IUserRepository, UserRepository>();

            // mediatR
            services.AddMediatR(typeof(GetAllUsersQuery).Assembly);
            services.AddMediatR(typeof(UpdateUserCommand).Assembly);
            services.AddMediatR(typeof(CreateUserCommand).Assembly);
            services.AddMediatR(typeof(DeleteUserCommand).Assembly);

            services.AddSingleton<IPasswordHasher<SignupUserCommand>, PasswordHasher<SignupUserCommand>>();
            services.AddSingleton<IPasswordHasher<SigninUserCommand>, PasswordHasher<SigninUserCommand>>();

            services.AddSwaggerGen(c =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, new string[] { } }
                });
            });
        }
    }
}
