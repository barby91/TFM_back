using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Models.DTO.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		private readonly IUserService _userService;

		public LoginController(IUserService userService)
		{
			_userService = userService;
		}

		/// <summary>
		/// Comprueba si los datos de login corresponden a un usuario existente
		/// </summary>
		/// <param name="user">Datos del login</param>
		/// <returns></returns>
		[HttpGet]
		public async Task<IActionResult> GetUser([FromQuery] UserLogginRequest user)
		{
			try
			{
				UserModel? userLogged = await _userService.GetUserByEmailAndPass(user.Email, user.Password);
				return Ok(userLogged);
			}
			catch(Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}
	}
}
