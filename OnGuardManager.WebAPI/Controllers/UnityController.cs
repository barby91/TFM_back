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

		/// <summary>
		/// Obtiene todas las unidades comunes de un determinado centro
		/// </summary>
		/// <param name="idCenter"></param>
		/// <returns></returns>
		[HttpGet()]
		public async Task<IActionResult> GetAllCommonUnity(int idCenter)
		{
			try
			{
				List<UnityModel> unitiesModel = await _unityService.GetAllCommonUnities(idCenter);
				return Ok(unitiesModel);
			}
			catch (Exception ex)
			{
				return BadRequest(JsonConvert.SerializeObject(ex.Message));
			}
		}

		/// <summary>
		/// Obtiene una unidad por su identificador
		/// </summary>
		/// <param name="idUnity">Identificador de la unidad</param>
		/// <returns></returns>
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

		/// <summary>
		/// Guarda una unidad nueva
		/// </summary>
		/// <param name="unityModel">Objeto unidad nueva</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> SaveNewUnity([FromBody] UnityModel unityModel)
		{
			try
			{
				bool result = await _unityService.AddUnity(unityModel.Map());
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Actualiza una unidad existente
		/// </summary>
		/// <param name="unityModel">Datos de la unidad a actualizar</param>
		/// <returns></returns>
		[HttpPut]
		public async Task<IActionResult> UpdateUnity([FromBody] UnityModel unityModel)
		{
			try
			{
				bool result =  await _unityService.UpdateUnity(unityModel.Map());
				return Ok(result);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Guarda una lista de unidades desde un fichero
		/// </summary>
		/// <param name="idCenter">identificador del centro</param>
		/// <param name="file">fichero donde están los datos</param>
		/// <returns></returns>
		[HttpPost("{idCenter}")]
		public async Task<IActionResult> SaveUnitiess(int idCenter, [FromForm] IFormFile file)
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
						return BadRequest("Se ha producido un error al guardar las unidades.");
					}
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Borra una unidad por su id
		/// </summary>
		/// <param name="id">Identificador de la unidad a borrar</param>
		/// <returns></returns>
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUnity(int id)
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
