using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Configuration;
using TodoApp.Models.DTOs.Requests;
using TodoApp.Models.DTOs.Responses;
using TodoApp.Data;
using Models;



namespace TodoApp.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("api/[controller]")] // api/authManagement
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly ApiDbContext _context;



        public AuthManagementController(
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor, ApiDbContext context)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _context = context;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if(ModelState.IsValid)
            {
                // We can utilise the model
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if(existingUser != null)
                {
                    return BadRequest(new RegistrationResponse(){
                            Errors = new Dictionary<string, List<string>>() {
                                {"Email", new List<string>(){"Email already in use"}},
                            },
                                                   
                            Success = false
                    });
                }

                var newUser = new IdentityUser() { Email = user.Email, UserName = user.Username};
                var isCreated = await _userManager.CreateAsync(newUser, user.Password);

 
                if(isCreated.Succeeded)
                {
                   var roleresult = await _userManager.AddToRoleAsync(newUser, "Student");
                   var jwtToken =  await GenerateJwtToken( newUser);

                   await _context.Students.AddAsync(new StudentDetails{Id = user.Email, DOB = "", FirstName = "", LastName = "", Semester = 1});
                   await _context.SaveChangesAsync();
                   

                   return Ok(new RegistrationResponse() {
                       Success = true,
                       Token = jwtToken
                   });
                } else {
                    return BadRequest(new RegistrationResponse(){
                            Errors = new Dictionary<string, List<string>>() {{ "Errors", isCreated.Errors.Select(x => x.Description).ToList() }},
                            Success = false
                    });
                }
            }

            return BadRequest(new RegistrationResponse(){
                    Errors = new Dictionary<string, List<string>>() {{ "Errors" , new List<string>() {
                        "Invalid payload"
                    } }},
                    Success = false
            });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if(ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if(existingUser == null) {
                        return BadRequest(new RegistrationResponse(){
                            Errors = new Dictionary<string, List<string>>() {{ "Errors", new List<string>() {
                                "User does not exist!"
                            } }},
                            Success = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if(!isCorrect) {
                      return BadRequest(new RegistrationResponse(){
                            Errors = new Dictionary<string, List<string>>() {{ "Errors", new List<string>() {
                                "Incorrect password!"
                            } }},
                            Success = false
                    });
                }

                var jwtToken  = await GenerateJwtToken(existingUser);

                return Ok(new RegistrationResponse() {
                    Success = true,
                    Token = jwtToken
                });
            }

            return BadRequest(new RegistrationResponse(){
                    Errors = new Dictionary<string, List<string>>() {{ "Errors", new List<string>() {
                        "Invalid payload"
                    } }},
                    Success = false
            });
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var roles = await _userManager.GetRolesAsync(user);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim("Id", user.Id), 
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, roles[0])
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}