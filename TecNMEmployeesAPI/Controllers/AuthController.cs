using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Helpers;
using TecNMEmployeesAPI.Services;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : CustomBaseController
    {
        private readonly UserManager<IdentityUser> UserManager;
        private readonly IConfiguration Configuration;
        private readonly SignInManager<IdentityUser> SignInManager;
        private readonly IDataProtector DataProtector;
        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public AuthController(UserManager<IdentityUser> userManager,
                IConfiguration configuration,
                SignInManager<IdentityUser> signInManager,
                IDataProtectionProvider dataProtectionProvider,
                ApplicationDbContext context,
                IMapper mapper) : base(context, mapper)
        {
            UserManager = userManager;
            Configuration = configuration;
            SignInManager = signInManager;
            DataProtector = dataProtectionProvider.CreateProtector("super_secreto");
            Context = context;
            Mapper = mapper;
        }




        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(UserCredentialsDTO userCredentials)
        {
            var user = new IdentityUser
            {
                UserName = userCredentials.UserName,
                Email = userCredentials.UserName
            };
            var result = await UserManager.CreateAsync(user, userCredentials.Password);

            if (result.Succeeded)
            {
                return await CreateToken(userCredentials);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }





        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(UserCredentialsDTO userCredentials)
        {
            var result = await SignInManager.PasswordSignInAsync(userCredentials.UserName,
                userCredentials.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await CreateToken(userCredentials);
            }
            else
            {
                return BadRequest("Credenciales incorrectas.");
            }
        }







        private async Task<AuthResponseDTO> CreateToken(UserCredentialsDTO userCredentials)
        {
            var claims = new List<Claim>()
            {
                new Claim("username", userCredentials.UserName )
            };

            var user = await UserManager.FindByNameAsync(userCredentials.UserName);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            var claimsdb = await UserManager.GetClaimsAsync(user);

            // Juntar las dos listas, los claims y los claimsdb
            // Añadir los claimsdb a claims
            claims.AddRange(claimsdb);


            // Crear el JWT
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:SEED"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(14);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expires, signingCredentials: credentials);

            var rolesDTO = Mapper.Map<List<RoleDetailDTO>>(claimsdb);

            return new AuthResponseDTO()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expires = expires,
                User = new UserDetailDTO()
                {
                    Roles = rolesDTO,
                    UserName = user.UserName,
                    Id = user.Id
                }
            };
        }





        [HttpGet("renew")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<AuthResponseDTO>> Renew()
        {
            var userName = HttpContext.User.Claims.Where(claim => claim.Type == "username").FirstOrDefault().Value;

            var userCredentials = new UserCredentialsDTO()
            {
                UserName = userName
            };

            return await CreateToken(userCredentials);
        }




        [HttpPost("changePassword")]
        public async Task<ActionResult<ResponseMessageDTO>> ChangePassword(UserChangePasswordDTO userChangePasswordDTO)
        { 
            
            var user = await UserManager.FindByNameAsync(userChangePasswordDTO.UserName); 

            if ( user == null )
            {
                return BadRequest($"El usuario con el UserName {userChangePasswordDTO.UserName} no existe.");
            }

            var result = await UserManager.ChangePasswordAsync(user, userChangePasswordDTO.OldPassword, userChangePasswordDTO.NewPassword);

            if (result.Succeeded)
            {
                return new ResponseMessageDTO()
                {
                    Message = "Contraseña actualizada con éxito"
                };
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }




        [HttpGet("users")]
        public async Task<ActionResult<PaginationResultDTO<UserDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Users.AsQueryable();

            var users = await queryable.Paginar(paginationDto)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var usersDTOs = Mapper.Map<List<UserDTO>>(users);

            var result = new PaginationResultDTO<UserDTO>
            {
                Results = usersDTOs,
                TotalResults = totalResults,
            };

            return result;
        }




        [HttpGet("user/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserDetailDTO>> GetUserById(string userId)
        {
          
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"No existe el usuario con el ID {userId}");
            }

            var claimsdb = await UserManager.GetClaimsAsync(user);

            var rolesDTO = Mapper.Map<List<RoleDetailDTO>>(claimsdb);

            return new UserDetailDTO()
            {
                Roles = rolesDTO,
                UserName = user.UserName,
                Id = userId
            };
        }


        [HttpPost("addroles")]
        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Sudo")]
        public async Task<ActionResult<List<RoleDetailDTO>>> AsignarRol(EditRoleDTO editRoleDTO)
        {
            var user = await UserManager.FindByIdAsync(editRoleDTO.UserId);
            if (user == null)
            {
                return NotFound($"No existe el usuario con el ID {editRoleDTO.UserId}");
            }

            if (editRoleDTO.Roles == null || editRoleDTO.Roles.LongCount() == 0 )
            {
                return BadRequest("El listado de roles es requerido.");
            }


            var claimsdb = await UserManager.GetClaimsAsync(user);
            await UserManager.RemoveClaimsAsync(user, claimsdb);

            var claims = new List<Claim>();
            for (int i = 0; i < editRoleDTO.Roles.LongCount(); i++)
            {
                claims.Add(new Claim(editRoleDTO.Roles[i].Role, editRoleDTO.Roles[i].Value));
            }

            await UserManager.AddClaimsAsync(user, claims);

            //await UserManager.AddClaimAsync(user, new Claim(editRoleDTO.Role, editRoleDTO.Value));
            var rolesDTO = Mapper.Map<List<RoleDetailDTO>>(claimsdb);

            return rolesDTO;

        }





        [HttpPost("removerole")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> RemoverRol(EditRoleDTO editRoleDTO)
        {
            var user = await UserManager.FindByIdAsync(editRoleDTO.UserId);
            if (user == null)
            {
                return NotFound($"No existe el usuario con el ID {editRoleDTO.UserId}");
            }

            // await UserManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, editRoleDTO.Role));
            //await UserManager.RemoveClaimAsync(user, new Claim(editRoleDTO.Role, editRoleDTO.Value));

            return NoContent();
        }




    }

}
