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

		// POST api/<LoginController>
		[HttpPost]
		public async Task<IActionResult> SaveSpecialty([FromBody] SpecialtyModel specialtyModel)
		{
			try
			{
				bool result = true;
				if (specialtyModel.Id == 0)
				{
					result = await _specialtyService.AddSpecialty(specialtyModel.Map());
				}
				else
				{
					result = await _specialtyService.UpdateSpecialty(specialtyModel.Map());
				}
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		[HttpPost("{idCenter}")]
		public async Task<IActionResult> SaveUsers(int idCenter, [FromForm] IFormFile file)
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
