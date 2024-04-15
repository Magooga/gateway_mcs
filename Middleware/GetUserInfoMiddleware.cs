


using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gateway.Middleware
{
    public class GetUserInfoMiddleware
    {
        private readonly RequestDelegate _next;

        public GetUserInfoMiddleware(RequestDelegate next) 
        { 
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try 
            {
                if (context.Request.Headers.TryGetValue("Authorization", out StringValues values))
                {
                    string? bearerToken = values.FirstOrDefault();
                    var res = GetSubFromBearerToken(bearerToken);

                    context.Request.Headers["userId"] = res;
                }

                Console.WriteLine("In road Custom middleware");

                await _next(context);

                Console.WriteLine("Out road Custom middleware");

            } catch (Exception ex) 
            { 
            
            }
        }

        public string? GetSubFromBearerToken(string? tokenString)
        {
            if (string.IsNullOrEmpty(tokenString) || !tokenString.Contains("Bearer"))
            {
                return null;
            }
            tokenString = tokenString.Split("Bearer ")[1];
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = jwtSecurityTokenHandler.ReadJwtToken(tokenString);

            // in Identity change Key name were save nameidentifier ????
            return token.Payload.TryGetValue(ClaimTypes.NameIdentifier, out object? value) ? (string)value : null;
        }
    }
}
