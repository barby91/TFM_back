using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Models.DTO.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RolController : ControllerBase
	{
		private readonly IRolService _rolService;

		public RolController(IRolService rolService)
		{
			_rolService = rolService;
		}

		[HttpGet()]
		public async Task<IActionResult> GetAllRols()
		{
			try
			{
				List<RolModel> rolModel = await _rolService.GetAllRols();
				return Ok(rolModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}
	}
}
