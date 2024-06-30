using Azure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Models.DTO.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GuardAssignmentController : ControllerBase
	{
		private readonly IDayGuardService _dayGuardService;

		public GuardAssignmentController(IDayGuardService dayGuardService)
		{
			_dayGuardService = dayGuardService;
		}

		[HttpPost()]
		public async Task<IActionResult> CalculateGuards([FromBody] GuardRequest guardRequest)
		{
			try
			{
				
				string result = await _dayGuardService.GetUserStats(guardRequest);

				if (result.Contains("OK"))
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(JsonConvert.SerializeObject(result));
				}
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.ToString()));
			}
		}

		[HttpGet()]
		public async Task<IActionResult> GetGuards(int idCenter)
		{
			try
			{
				DateTime currentDate = DateTime.Now.Date;
				List<DayGuardModel> dayGuardsModel = await _dayGuardService.GetGuards(idCenter, currentDate.Year);
				
				return Ok(dayGuardsModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.ToString()));
			}
		}
	}
}
