using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Logger;
using onGuardManager.Models.DTO.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UnityController : ControllerBase
	{
		private readonly IUnityService _unityService;

		public UnityController(IUnityService unityService)
		{
			_unityService = unityService;
		}

		[HttpGet()]
		public async Task<IActionResult> GetAllCommonUnity()
		{
			try
			{
				List<UnityModel> unitiesModel = await _unityService.GetAllCommonUnities();
				return Ok(unitiesModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		[HttpGet("{idUnity}")]
		public async Task<IActionResult> GetUnityById(int idUnity)
		{
			try
			{
				UnityModel? unity = await _unityService.GetUnityById(idUnity);
				return Ok(unity);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		// POST api/<LoginController>
		[HttpPost]
		public async Task<IActionResult> SaveUnity([FromBody] UnityModel unityModel)
		{
			try
			{
				bool result = true;
				if (unityModel.Id == 0)
				{
					result = await _unityService.AddUnity(unityModel.Map());
				}
				else
				{
					result = await _unityService.UpdateUnity(unityModel.Map());
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
					bool result = await _unityService.AddUnities(reader);

					if (result)
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
				bool result = await _unityService.DeleteUnity(id);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
