using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Logger;
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

		[HttpGet("{idCenter}")]
		public async Task<IActionResult> GetAllUsersBycenter(int idCenter)
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

		// POST api/<LoginController>
		[HttpPost]
		public async Task<IActionResult> SaveUser([FromBody] RealUserModel userModel)
		{
			try
			{
				bool result = true;
				if (userModel.Id == 0)
				{
					result = await _userService.AddUser(userModel.Map());
				}
				else
				{
					result = await _userService.UpdateUser(userModel.Map());
				}
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

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

		// DELETE api/<LoginController>/5
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
