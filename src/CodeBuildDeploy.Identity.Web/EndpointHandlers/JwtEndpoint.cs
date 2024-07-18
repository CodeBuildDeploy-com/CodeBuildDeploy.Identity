using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using CodeBuildDeploy.Identity.DA.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CodeBuildDeploy.Identity.Web.EndpointHandlers
{
    internal class JWTEndpoint
    {
        public static string? GetTokenHandler(
            HttpContext context, 
            [FromServices] IConfiguration configuration)
        {
            return CreateJwtToken(
                context.User.Identities.First(), 
                $"{context.Request.Scheme}://{context.Request.Host}", 
                configuration);
        }

        public static IResult GetLoginHandler()
        {
            return Results.Content(
                $"""
                    <form method="post">
                        <div>Email:<input id="email" name="email"></input></div>
                        <div>Password:<input id="password" name="password" type="password"></input></div>
                        <div><button type="submit">Sign in</button></div>
                    </form>
                """, "text/html");
        }

        public static async Task<string?> LoginHandler(
            [FromForm] string email,
            [FromForm] string password,
            HttpContext context,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] IUserClaimsPrincipalFactory<ApplicationUser> claimsPrincipalFactory,
            [FromServices] IConfiguration configuration)
        {
            var user = await userManager.FindByNameAsync(email);
            if (user != null)
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    user.LastLoginTime = DateTime.UtcNow;
                    await userManager.UpdateAsync(user);

                    var claimsPrincipal = await claimsPrincipalFactory.CreateAsync(user);
                    var claimsIdentity = claimsPrincipal.Identities.First();
                    claimsIdentity.AddClaim(new Claim("amr", "pwd"));
                    claimsIdentity.AddClaim(new Claim("method", "jwt"));
                    return CreateJwtToken(claimsIdentity, $"{context.Request.Scheme}://{context.Request.Host}", configuration);
                }
            }

            return "Unable to login with provided user details";
        }
        private static string? CreateJwtToken(
            ClaimsIdentity user,
            string issuer,
            IConfiguration configuration)
        {
            var jwtAuthSection = configuration.GetSection("Authentication:Jwt");
            if (jwtAuthSection.Exists() && jwtAuthSection.GetChildren().Any(x => x.Key == "PrivateKey"))
            {
                JsonWebKeySet jwks = new(jwtAuthSection["PrivateKey"]!);
                var signingKeys = jwks.GetSigningKeys();

                var handler = new JsonWebTokenHandler();

                var token = handler.CreateToken(new SecurityTokenDescriptor()
                {
                    Issuer = issuer,
                    Subject = user,
                    SigningCredentials = new SigningCredentials(signingKeys[0], SecurityAlgorithms.RsaSha256)
                });
                return token;
            }
            return "No Private Key Configured";
        }
    }
}
