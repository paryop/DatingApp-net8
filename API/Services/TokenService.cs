using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService(IConfiguration config, UserManager<AppUser> userManager) : ITokenService
    {
        public async Task<string> CreateToken(AppUser appUser)
        {
            var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access tokenkey from appsettings!!!");

            if(tokenKey.Length < 64) throw new Exception("Your token key should be longer!!!");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            if(appUser.UserName == null) throw new Exception("No username for user found!!!");
            
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                new (ClaimTypes.Name, appUser.UserName),
            };

            var roles = await userManager.GetRolesAsync(appUser);

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}