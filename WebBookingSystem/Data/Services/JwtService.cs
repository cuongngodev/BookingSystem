using BookingSystem.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebBookingSystem.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config; // Configuration object to access appsettings.json

        public JwtService(IConfiguration configuration)
        {
            _config = configuration;
        }

        // Generates a JWT token for a given user
        public string GenerateToken(ApplicationUser user)
        {
            // Get the "Jwt" section from appsettings.json
            var jwtSection = _config.GetSection("Jwt");

            // Create a symmetric security key using the secret key from config
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));

            // Define claims to include in the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                new Claim(JwtRegisteredClaimNames.Email, user.Email),       
                new Claim("firstName", user.FirstName),                     
                new Claim("lastName", user.LastName)       
            };

            // Create signing credentials using HMAC-SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token object
            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],           
                audience: jwtSection["Audience"],       
                claims: claims,                          
                expires: DateTime.UtcNow.AddMinutes(60), 
                signingCredentials: creds              
            );

            // Return the serialized JWT token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Validates a JWT token and returns the user ID (Subject) if valid
        public string? ValidateToken(string token)
        {
            try
            {
                // Get JWT settings
                var jwtSection = _config.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));

                // Create a token handler to validate the token
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,                 
                    ValidateAudience = true,              
                    ValidateIssuerSigningKey = true,     
                    ValidIssuer = jwtSection["Issuer"],    
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = key,                
                    ValidateLifetime = true,               
                    ClockSkew = TimeSpan.Zero              
                }, out SecurityToken validatedToken);

                // Cast the validated token to JwtSecurityToken
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Return the subject claim (user ID)
                return jwtToken.Subject;
            }
            catch
            {
                // Token invalid or expired
                return null;
            }
        }
    }
}
