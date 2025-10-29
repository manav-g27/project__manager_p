using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Api.Models;

namespace ProjectManager.Api.Auth
{
    public static class Jwt
    {
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var h = new HMACSHA256();
            salt = h.Key; hash = h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
        public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var h = new HMACSHA256(salt);
            var comp = h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return comp.SequenceEqual(hash);
        }
        public static string CreateToken(User user, string secret)
        {
            var creds = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Username) };
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
