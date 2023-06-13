using JWT.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorizeController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthorizeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            return "Auth Controller accessed in " + DateTime.Now.ToLongTimeString();
        }
        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] UserDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);
            await _signInManager.SignInAsync(user, false);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserDTO userInfo)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
            var result = await _signInManager.PasswordSignInAsync(userInfo.Email,
                userInfo.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login");
                return BadRequest(ModelState);
            }
            return Ok();
        }
        private UserToken GenerateToken(UserDTO userInfo)
        {
            //User declaration
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
                new Claim("myPet", "Amora"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //Getting token key setted in appsettings.json
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Token expiration time
            var expirationTime = _config["TokenConfiguration:ExpireHours"];
            var expirationDate = DateTime.UtcNow.AddHours(double.Parse(expirationTime));

            //Generating token
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config["TokenConfiguration:Issuer"],
                audience: _config["TokenConfiguration:Audience"],
                claims: claims,
                expires: expirationDate,
                signingCredentials: credentials
                );

            //Return UserToken
            return new UserToken()
            {
                Authenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expirationDate,
                Message = "JWT Token OK"
            };
        }
    }
}
