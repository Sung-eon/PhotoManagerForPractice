namespace PhotoManager.Api.v1.Helper.Auth;

using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

public class TokenIssuer
{
    private readonly IConfiguration _configuration;
    public TokenIssuer(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public string GetToken(string username, IEnumerable<Claim>? additionalClaims, int expiresIn = 90)
    {
        Claim[] claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (additionalClaims is not null)
        {
            foreach (var item in additionalClaims)
            {
                claims.Append(item);
            }
        }   

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["JWT:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: this._configuration["JWT:ValidIssuer"],
            audience: this._configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddMinutes(expiresIn),
            signingCredentials: creds,
            claims: claims
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GetBearerToken(string username, IEnumerable<Claim>? additionalClaims = null, int expiresIn = 90)
    {
        string Token = this.GetToken(
            username: username, 
            additionalClaims: additionalClaims, 
            expiresIn: expiresIn
        );
        
        return $"Bearer {Token}";
    }
}