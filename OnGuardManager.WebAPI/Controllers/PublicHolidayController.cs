using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using onGuardManager.Bussiness.IService;
using onGuardManager.Models.DTO.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PublicHolidayController : ControllerBase
	{
		private readonly IPublicHolidayService _publicHolidayService;

		public PublicHolidayController(IPublicHolidayService publicHolidayService)
		{
			_publicHolidayService = publicHolidayService;
		}

		[HttpGet("{idCenter}")]
		public async Task<IActionResult> GetAllPublicHolidays(int idCenter)
		{
			try
			{
				List<PublicHolidayModel> publicHolidaysModel = await _publicHolidayService.GetAllPublicHolidaysByCenter(idCenter);
				return Ok(publicHolidaysModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}
	}
}
