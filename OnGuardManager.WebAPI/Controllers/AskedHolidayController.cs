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
		private readonly IPublicHolidayService _publicHolidayService;
		private readonly IUserService _userService;

		public AskedHolidayController(IAskedHolidayService askedHolidayService, IHolidayStatusService holidayStatusHolidayService,
									  IPublicHolidayService publicHolidayService, IUserService userService)
		{
			_askedHolidayService = askedHolidayService;
			_holidayStatusHolidayService = holidayStatusHolidayService;
			_publicHolidayService = publicHolidayService;
			_userService = userService;
		}

		/// <summary>
		/// Crea una nueva solicitud de vacaciones o fin de semana
		/// </summary>
		/// <param name="askedHolidayModel">Datos de los días solicitados</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> SaveNewHolidayAsked([FromBody] AskedHolidayModel askedHolidayModel)
		{
			try
			{
				bool result = true;
				string message = "No se han podido solicitar los días, compruebe que no los tenga ya solicitados";
				
				//si ha pedido nuevas vacaciones, primero hay que comprobar que puede pedirlas, es decir, que le quedan días suficientes
				if (_askedHolidayService.CheckPendingHolidaysUser(askedHolidayModel))
				{
					AskedHolidayModel? ahm = await _askedHolidayService.UpdateCancelAskedHoliday(askedHolidayModel);
					if (ahm != null)
					{
						askedHolidayModel = ahm;
					}

					//puede ser necesario actualizar en lugar de crear un nuevo objeto si la solicitud está cancelada
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
					if (!askedHolidayModel.Period.Equals("Weekend") && await _holidayStatusHolidayService.GetIdHolidayStatusByDescription(askedHolidayModel.StatusDes) == (int)EnumHolidayStatus.Solicitado)
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

		/// <summary>
		/// Actualiza una petición de vacaciones
		/// </summary>
		/// <param name="askedHolidayModel"></param>
		/// <returns></returns>
		[HttpPut]
		public async Task<IActionResult> UpdateHolidayAsked([FromBody] AskedHolidayModel askedHolidayModel)
		{
			try
			{
				bool result = true;
				string message = "No se han podido solicitar los días, compruebe que no los tenga ya solicitados";

				//si ha pedido nuevas vacaciones, primero hay que comprobar que puede pedirlas, es decir, que le quedan días suficientes
				result = await _askedHolidayService.UpdateAskedHoliday(askedHolidayModel.Map(), 
																		await _holidayStatusHolidayService.GetIdHolidayStatusByDescription(askedHolidayModel.StatusDes));
					
				//Es necesario comprobar si hay un fin de semana por medio, en cuyo caso, se solicita automáticamente.
				//Esta comprobación solo se hace si el periodo no es weekend
				if (!askedHolidayModel.Period.Equals("Weekend") &&
					await _holidayStatusHolidayService.GetIdHolidayStatusByDescription(askedHolidayModel.StatusDes) == (int)EnumHolidayStatus.Solicitado)
				{
					result = result && await CheckWeekend(askedHolidayModel);
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

		/// <summary>
		/// Obtiene los días solictidaos pendientes de aprobación de los usuarios de un determinado centro,
		/// excepto los del usuario logado.
		/// </summary>
		/// <param name="idCenter">Identificador del centro</param>
		/// <param name="idUser">Identificador del usuario</param>
		/// <returns></returns>
		[HttpGet("{idCenter}")]
		public async Task<IActionResult> GetAllPendingAskedHolidays(int idCenter, int idUser)
		{
			try
			{
				List<PendingAskedHolidayModel> askedHolidaysModel = 
					await _askedHolidayService.GetAllPendingAskedHolidaysByCenter(idCenter, 
																				  idUser,
																				  EnumHolidayStatus.Solicitado.ToString());
				return Ok(askedHolidaysModel);
			}
			catch(Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		#region private methods

		/// <summary>
		/// Este método comprueba si hay algún fin de semana o festivo entre los días solicitados
		/// </summary>
		/// <param name="askedHolidayModel">Días solicitados</param>
		/// <returns></returns>
		private async Task<bool> CheckWeekend (AskedHolidayModel askedHolidayModel)
		{
			DateOnly date = askedHolidayModel.DateFrom;
			bool findWeekEnd = false;
			bool result = true;
			AskedHolidayModel askedWeekend = new AskedHolidayModel();
			List<PublicHolidayModel> publicHoliday = await _publicHolidayService.GetAllPublicHolidaysByCenter(askedHolidayModel.IdCenter);
			List<DateOnly> datePublicHoliday = publicHoliday.Select(ph => ph.Date).ToList();

			while (date < askedHolidayModel.DateTo && !findWeekEnd)
			{
				//solo busca por sábado porque busamos el primer día de fin de semana del periodo.
				//Esto es sábado ya que el periodo será de días laborables. Buscamos también si
				//hay un día festivo
				if (date.DayOfWeek == DayOfWeek.Saturday || datePublicHoliday.Contains(date))
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
				//En este caso será domingo o el último día festivo
				findWeekEnd = false;
				date = askedHolidayModel.DateTo;
				while (date >= askedHolidayModel.DateFrom && !findWeekEnd)
				{
					if (date.DayOfWeek == DayOfWeek.Sunday || datePublicHoliday.Contains(date))
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

		#endregion
	}
}
