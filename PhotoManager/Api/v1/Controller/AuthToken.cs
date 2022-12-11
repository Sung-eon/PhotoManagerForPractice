namespace PhotoManager.Api.v1.Controller;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PhotoManager.Api.v1.Helper.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

public class LoginModel
{
    public string username { get; init; }
    public string password { get; init; }
}

[ApiController]
[Route("api/v1/[controller]")]
public class AuthToken : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly TokenIssuer _tokenIssuer;
    public AuthToken(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, 
                        IConfiguration configuration, TokenIssuer tokenIssuer)
    {
        this._userManager = userManager;
        this._signInManager = signInManager;
        this._configuration = configuration;
        this._tokenIssuer = tokenIssuer;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromForm] LoginModel login)
    {
        IdentityUser user = await this._userManager.FindByNameAsync(login.username);
        bool IsValidate = await _userManager.CheckPasswordAsync(user, login.password);

        var GetUserRoles = await _userManager.GetRolesAsync(user);
        bool IsAdminUser = GetUserRoles.Contains("Administrator");

        if (IsValidate && IsAdminUser)
        {
            var additionalClaims = await _userManager.GetClaimsAsync(user);

            var token = this._tokenIssuer.GetToken(username: login.username, additionalClaims: additionalClaims);
            return Ok(token);
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult LoginTest()
    {
        return Ok("Good");
    }   

    
}