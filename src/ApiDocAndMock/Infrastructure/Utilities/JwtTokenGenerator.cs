using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Utilities
{
    public static class JwtTokenGenerator
    {
        public static string GenerateMockJwt(string[] roles)
        {
            var key = Encoding.UTF8.GetBytes("verylongsupersecurekey12345678forHmacSha256");
            var signingKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);


            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "mock-user"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, "mock-api"),
                new Claim(JwtRegisteredClaimNames.Aud, "mock-clients")
            };

            // Add roles to the token
            var listRoles = roles.ToList();
            listRoles.ForEach(role =>
            {
                claims.Add(new Claim("role", role));
            });

            var token = new JwtSecurityToken(
                issuer: "mock-api",
                audience: "mock-clients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
