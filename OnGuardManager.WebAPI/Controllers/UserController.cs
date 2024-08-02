using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Models.DTO.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		/// <summary>
		/// Obtiene todos los usuarios por centro
		/// </summary>
		/// <param name="idCenter"></param>
		/// <returns></returns>
		[HttpGet("{idCenter}")]
		public async Task<IActionResult> GetAllUsersByCenter(int idCenter)
		{
			try
			{
				List<UserModel> usersModel = await _userService.GetAllUsersByCenter(idCenter);
				return Ok(usersModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		/// <summary>
		/// Obtiene un usuario por su identificador
		/// </summary>
		/// <param name="id"></param>
		/// <param name="idCenter"></param>
		/// <returns></returns>
		[HttpGet()]
		public async Task<IActionResult> GetUserById(int id, int idCenter)
		{
			try
			{
				RealUserModel? user = await _userService.GetUserById(id);
				return Ok(user);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		/// <summary>
		/// Guarda un usuario nuevo
		/// </summary>
		/// <param name="userModel">Datos del nuevo usuario</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> SaveNewUser([FromBody] RealUserModel userModel)
		{
			try
			{
				bool result = await _userService.AddUser(userModel.Map());
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Actualiza un usaurio existente
		/// </summary>
		/// <param name="userModel">Datos del usuario a actualizar</param>
		/// <returns></returns>
		[HttpPut]
		public async Task<IActionResult> UpdateUser([FromBody] RealUserModel userModel)
		{
			try
			{
				bool result = await _userService.UpdateUser(userModel.Map());
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Guarda una lista de usuario nuevos cuyos datos están en un fichero
		/// </summary>
		/// <param name="idCenter">Identificador del centro</param>
		/// <param name="file">Fichero con los datos de los usuarios</param>
		/// <returns></returns>
		[HttpPost("{idCenter}")]
		public async Task<IActionResult> SaveUsers(int idCenter, [FromForm] IFormFile file)
		{
			try
			{
				using (var reader = new StreamReader(file.OpenReadStream()))
				{
					bool result = await _userService.AddUsers(reader, idCenter);

					if(result)
					{
						return Ok(true);
					}
					else
					{
						return BadRequest("Se ha producido un error al guardar los usuarios.");
					}
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Borra un usuario existente
		/// </summary>
		/// <param name="id">Identificador del usuario a borrar</param>
		/// <returns></returns>
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				bool result = await _userService.DeleteUser(id);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
