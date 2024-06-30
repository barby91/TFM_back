using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Models.DTO.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LevelController : ControllerBase
	{
		private readonly ILevelService _levelService;

		public LevelController(ILevelService levelService)
		{
			_levelService = levelService;
		}

		[HttpGet()]
		public async Task<IActionResult> GetAllLevels()
		{
			try
			{
				List<LevelModel> levelModel = await _levelService.GetAllLevels();
				return Ok(levelModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}
	}
}
