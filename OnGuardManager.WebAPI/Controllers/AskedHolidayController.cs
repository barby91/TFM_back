using Microsoft.AspNetCore.Mvc;
using onGuardManager.Bussiness.IService;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.DTO.Enumerados;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AskedHolidayController : ControllerBase
	{
		private readonly IAskedHolidayService _askedHolidayService;
		private readonly IHolidayStatusService _holidayStatusHolidayService;
		private readonly IUserService _userService;

		public AskedHolidayController(IAskedHolidayService askedHolidayService, IHolidayStatusService holidayStatusHolidayService, 
									  IUserService userService)
		{
			_askedHolidayService = askedHolidayService;
			_holidayStatusHolidayService = holidayStatusHolidayService;
			_userService = userService;
		}

		[HttpPost]
		public async Task<IActionResult> SaveHolidayAsked([FromBody] AskedHolidayModel askedHolidayModel)
		{
			try
			{
				bool result = true;
				string message = "No se han podido solicitar los días, compruebe que no los tenga ya solicitados";
				
				//si ha pedido nuevas vacaciones, primero hay que comprobar que puede pedirlas, es decir, que le quedan días suficientes
				if (_askedHolidayService.CheckPendingHolidaysUser(askedHolidayModel))
				{
					if (askedHolidayModel.Id == 0)
					{
						AskedHolidayModel? ahm = await _askedHolidayService.UpdateCancelAskedHoliday(askedHolidayModel);
						if (ahm != null)
						{
							askedHolidayModel = ahm;
						}
					}
					if (askedHolidayModel.Id == 0)
					{
						result = await _askedHolidayService.AddAskedHoliday(askedHolidayModel.Map(), await _holidayStatusHolidayService.GetIdHolidayStatusByDescription(EnumHolidayStatus.Solicitado.ToString()));

					}
					else
					{
						result = await _askedHolidayService.UpdateAskedHoliday(askedHolidayModel.Map(), await _holidayStatusHolidayService.GetIdHolidayStatusByDescription(askedHolidayModel.StatusDes));
					}

					//Es necesario comprobar si hay un fin de semana por medio, en cuyo caso, se solicita automáticamente.
					//Esta comprobación solo se hace si el periodo no es weekend
					if (!askedHolidayModel.Period.Equals("Weekend"))
					{
						result = result && await CheckWeekend(askedHolidayModel);
					}
				}
				else
				{
					result = false;
					message = "Ya ha solicitado todos los días de este periodo";
				}
				
				if (result)
				{
					return Ok(await _userService.GetUserModelById(askedHolidayModel.IdUser));
				}
				else
				{
					return BadRequest(JsonConvert.SerializeObject(message));
				}
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		[HttpGet("{idCenter}")]
		public async Task<IActionResult> GetAllPendingAskedHolidays(int idCenter, int idUser)
		{
			try
			{
				List<PendingAskedHolidayModel> askedHolidaysModel = await _askedHolidayService.GetAllPendingAskedHolidaysByCenter(idCenter, idUser, EnumHolidayStatus.Solicitado.ToString());
				return Ok(askedHolidaysModel);
			}
			catch(Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		private async Task<bool> CheckWeekend (AskedHolidayModel askedHolidayModel)
		{
			DateOnly date = askedHolidayModel.DateFrom;
			bool findWeekEnd = false;
			bool result = true;
			AskedHolidayModel askedWeekend = new AskedHolidayModel();

			while (date < askedHolidayModel.DateTo && !findWeekEnd)
			{
				//solo busca por sábado porque busamos el primer día de fin de semana del periodo.
				////Esto es sábado ya que el periodo será de días laborables
				if (date.DayOfWeek == DayOfWeek.Saturday) 
				{
					findWeekEnd = true;
					askedWeekend = new AskedHolidayModel()
					{
						DateFrom = date,
						IdUser = askedHolidayModel.IdUser,
						Period = "Weekend",
						StatusDes = askedHolidayModel.StatusDes
					};
				}

				date = date.AddDays(1);
			}

			if (findWeekEnd)
			{
				//ahora buscamos el último día de fin de semana del periodo.
				//En este caso será domingo
				findWeekEnd = false;
				date = askedHolidayModel.DateTo;
				while (date >= askedHolidayModel.DateFrom && !findWeekEnd)
				{
					if (date.DayOfWeek == DayOfWeek.Sunday)
					{
						findWeekEnd = true;
						askedWeekend.DateTo = date;
					}

					date = date.AddDays(-1);
				}
			}

			//guardamos la solicitud de fin de semana, si es necesario
			if (findWeekEnd)
			{
				AskedHolidayModel? ahm = await _askedHolidayService.UpdateCancelAskedHoliday(askedWeekend);
				if (ahm != null)
				{
					askedWeekend = ahm;
				}
				if (askedWeekend.Id == 0)
				{
					result = await _askedHolidayService.AddAskedHoliday(askedWeekend.Map(), await _holidayStatusHolidayService.GetIdHolidayStatusByDescription(EnumHolidayStatus.Solicitado.ToString()));

				}
				else
				{
					result = await _askedHolidayService.UpdateAskedHoliday(askedWeekend.Map(), await _holidayStatusHolidayService.GetIdHolidayStatusByDescription(askedHolidayModel.StatusDes));
				}
			}
			return await Task.FromResult(result);
		}
	}
}
