using AutoMapper;
using HotelListing.Data;
using HotelListing.Models;
using HotelListing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager;
        //  private readonly SignInManager<ApiUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthManager _authManager;

        public AccountController(UserManager<ApiUser> userManager, ILogger<AccountController> logger, IMapper mapper, IAuthManager authManager)
        {
            _userManager = userManager;
            //_signInManager = signInManager;
            _logger = logger;
            _mapper = mapper;
            _authManager = authManager;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            _logger.LogInformation($"Registration attempt for {userDTO.Email}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = _mapper.Map<ApiUser>(userDTO);
                user.UserName = userDTO.Email; //UserName is required in ASP.Net
                var result = await _userManager.CreateAsync(user, userDTO.Password); //IMPORTANT: Make sure to include userDTO.Password or the password will be empty.

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors) //COULD CONTAIN SENSITIVE INFO!!!  This is mostly for testing reasons
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState); //BadRequest("User Registration Attempt Failed");
                }

                await _userManager.AddToRolesAsync(user, userDTO.Roles);
                return Accepted(); //Ok() is fine as well.
            }
            catch (Exception ex)
            {

                _logger.LogError($"Something went wrong while registering {userDTO.Email}: {ex}");
                return Problem($"Something went wrong while registering {userDTO.Email}", statusCode: 500);
            }
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO userDTO) //Not needed in MapperInitializer
        {
            _logger.LogInformation($"Login attempt for {userDTO.Email}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                if (!await _authManager.ValidateUser(userDTO))
                {
                    //var erros = result.errors;
                    return Unauthorized(userDTO);
                }

                return Accepted(new { Token = await _authManager.CreateToken() }); 
            }
            catch (Exception ex)
            {

                _logger.LogError($"Something went wrong while logging in  {userDTO.Email}: {ex}");
                return Problem($"Something went wrong while logging in {userDTO.Email}", statusCode: 500);
            }
        }
    }
}
