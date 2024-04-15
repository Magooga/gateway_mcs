using Gateway.Middleware;
//using JwtTokenAuthentication;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

using Gateway.ParceOcelot;

namespace Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string Origin = "MyAllowSpecificOrigins";

            var builder = WebApplication.CreateBuilder(args);

            /******* from Maria Mcs *****/
            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.FromMinutes(0), // !!! ����� ����� ��������, ��-�� ���������� ����������� ����� ������� � �������, ����� ���������� ���� ��������: https://stackoverflow.com/questions/61909514/jwt-token-expiration-not-working-in-asp-net-core-api

                        ValidateIssuerSigningKey = true,
                    
                        ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
                        ValidAudience = builder.Configuration["Jwt:Audience"]!,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
                    };
                });

            builder.Services.AddAuthorization(options => options.DefaultPolicy =
                new AuthorizationPolicyBuilder//��� ����� ����� �� ��������
                        (JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build());
            /**************************/

            //builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: Origin,
                                  b =>
                                  {
                                      b.WithOrigins(builder.Configuration.GetSection("CORS:Origins").Get<string[]>())
                                       .WithHeaders(builder.Configuration.GetSection("CORS:Headers").Get<string[]>())
                                       .WithMethods(builder.Configuration.GetSection("CORS:Methods").Get<string[]>());
                                  });
            });


            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            
            builder.Services.AddOcelot(builder.Configuration);

            //builder.Services.ConfigureDownstreamHostAndPortsPlaceholders();
            builder.Services.ConfigureDownstreamHostAndPortsPlaceholders(builder.Configuration);

            //builder.Services.AddJwtAuthentication(); // My extension method for authenticate politics, from JwtTokenAuthentication project for test authentication

            var app = builder.Build();

            //app.UseHttpsRedirection();


            //app.MapGet("/", () => "Hello World!");

            //app.MapControllers();


            app.UseCors(Origin);
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseGetUserInfo(); // My Custom MiddleWare to extract user info from jwt and join it to request header

            app.UseOcelot().Wait();

            


            app.Run();
        }
    }
}