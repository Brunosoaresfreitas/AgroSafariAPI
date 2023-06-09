﻿using AgroSafari.API.Filter;
using AgroSafari.Application.Commands.CreateClient;
using AgroSafari.Application.Validators;
using AgroSafari.Core.Repositories;
using AgroSafari.Core.Services;
using AgroSafari.Infrastructure;
using AgroSafari.Infrastructure.AuthServices;
using AgroSafari.Infrastructure.Persistence.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace DevFreela.API
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
            // Obtenção da string de conexão
            var connectionString = Configuration.GetConnectionString("AgroSafariCs");

            // Configuração do contexto do banco de dados
            services.AddDbContext<AgroSafariDbContext>(options =>
                options.UseInMemoryDatabase("AgroSafariDatabase"));

            //services.AddDbContext<AgroSafariDbContext>
            //    (options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("AgroSafari.API")));

            // Injeções de dependência dos respositórios, na qual sempre que eu referenciar uma Interface citada, utilizarei a classe que implemente os seus contratos;
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IServiceProviderRepository, ServiceProviderRepository>();
            services.AddScoped<IAuthService, AuthService>();


            services.AddControllers(options => options.Filters.Add(typeof(ValidationFilter)));
            // Adicionando o Filter para validação, isso faz com que o ValidationFilter definido na classe
            // seja executado automaticamente na aplicação sem precisar "colocar código" nos controllers.


            // Aqui estamos adicionando o FluentValidation contendo os Validators responsáveis por realizar as validações dos dados de entrada da API criados que
            // estão no mesmo assembly de CreateUserCommandValidator
            services.AddFluentValidation();
            services.AddValidatorsFromAssemblyContaining<CreateClientCommandValidator>();


            // Adicionando o Mediator que será injetado nos controllers 
            // Atua buscando todas as classes/comandos que implementem IRequest e associá-los ao commands handler que implementem IRequestHandler
            services.AddMediatR(typeof(CreateClientCommand));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AgroSafari.API",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Bruno Henrique",
                        Email = "brunosoaresfreitas26@gmail.com",
                        Url = new Uri("https://www.linkedin.com/in/bruno-henrique-soares-de-freitas-32ab85243/")
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header usando o esquema Bearer."
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
                             new string[] {}
                     }
                 });
            });

            services
              .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options =>
              {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidateAudience = true,
                      ValidateLifetime = true,
                      ValidateIssuerSigningKey = true,

                      ValidIssuer = Configuration["Jwt:Issuer"],
                      ValidAudience = Configuration["Jwt:Audience"],
                      IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                  };
              });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroSafari.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
