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
	public class SpecialtyController : ControllerBase
	{
		private readonly ISpecialtyService _specialtyService;

		public SpecialtyController(ISpecialtyService specialtyService)
		{
			_specialtyService = specialtyService;
		}

		/// <summary>
		/// Obtiene todas las especialidades de un dentro
		/// </summary>
		/// <param name="idCenter">Identificador del centro</param>
		/// <returns></returns>
		[HttpGet("{idCenter}")]
		public async Task<IActionResult> GetAllSpecialtiesBycenter(int idCenter)
		{
			try
			{
				List<SpecialtyModel> specialiesModel = await _specialtyService.GetAllSpecialtiesByCenter(idCenter);
				return Ok(specialiesModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		/// <summary>
		/// Obtiene una especialidad por su id
		/// </summary>
		/// <param name="id">Identificador de la especialidad que se busca</param>
		/// <returns></returns>
		[HttpGet()]
		public async Task<IActionResult> GetSpecialtyById(int id)
		{
			try
			{
				SpecialtyModel? specialty = await _specialtyService.GetSpecialtyById(id);
				return Ok(specialty);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		/// <summary>
		/// Guarda una nueva especialidad
		/// </summary>
		/// <param name="specialtyModel">Datos de la nueva especialidad</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> SaveNewSpecialty([FromBody] SpecialtyModel specialtyModel)
		{
			try
			{
				bool result = await _specialtyService.AddSpecialty(specialtyModel.Map());
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		/// <summary>
		/// Actualiza una especialidad existente
		/// </summary>
		/// <param name="specialtyModel">Datos de la especialidad a actualizar</param>
		/// <returns></returns>
		[HttpPut]
		public async Task<IActionResult> UpdateSpecialty([FromBody] SpecialtyModel specialtyModel)
		{
			try
			{
				bool result = await _specialtyService.UpdateSpecialty(specialtyModel.Map());
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		/// <summary>
		/// Guarda una lista de especialidades dadas por fichero
		/// </summary>
		/// <param name="idCenter">Identificador del centro</param>
		/// <param name="file">Fichero con los datos</param>
		/// <returns></returns>
		[HttpPost("{idCenter}")]
		public async Task<IActionResult> SaveSpecialties(int idCenter, [FromForm] IFormFile file)
		{
			try
			{
				List<SpecialtyModel> newSpecialties = new List<SpecialtyModel>();
				using (var reader = new StreamReader(file.OpenReadStream()))
				{
					while (reader.Peek() >= 0)
					{
						string? specialtyStr = reader.ReadLine();
						string[] specialtyArray = specialtyStr == null ? [] : specialtyStr.Split(';');
						if (specialtyArray.Length == 2)
						{
							newSpecialties.Add(new SpecialtyModel()
							{
								Name = specialtyArray[0],
								Description = specialtyArray[1],
								IdCenter = idCenter
							});
						}
						else
						{
							LogClass.WriteLog(ErrorWrite.Error, "Faltan o sobran datos ");
						}
					}

					bool result = await _specialtyService.AddSpecialties(newSpecialties);

					if (result)
					{
						return Ok(true);
					}
					else
					{
						return BadRequest("Se ha producido un error al guardar las especialidades.");
					}
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Borra una especialidad existente
		/// </summary>
		/// <param name="id">Identificador de la especialidad a borrar</param>
		/// <returns></returns>
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				bool result =  await _specialtyService.DeleteSpecialty(id);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
