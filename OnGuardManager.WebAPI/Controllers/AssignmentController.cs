using Microsoft.AspNetCore.Mvc;
using onGuardManager.Bussiness.IService;
using onGuardManager.WebAPI;
using System.Linq.Expressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnGuardManager.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AssignmentController : ControllerBase
	{
	//	private readonly IUserService _userService;

	//	public AssignmentController(IUserService userService)
	//	{
	//		_userService = userService;
	//	}

	//	[HttpGet()]
	//	public async Task<IActionResult> GetAllCommonUnity()
	//	{
	//		List<User> users = await _userService.GetAllUsersByCenter(1);

	//		List<Assignment> assignments = new List<Assignment>();
	//		assignments.Add(new Assignment()
	//		{
	//			Day = new DateOnly(2024, 5, 1),
	//			Users = users.Where(u => u.Id == 1 || u.Id == 2).ToList()
	//		});
	//		assignments.Add(new Assignment()
	//		{
	//			Day = new DateOnly(2024, 5, 2),
	//			Users = users.Where(u => u.Id == 1 || u.Id == 3).ToList()
	//		});
	//		assignments.Add(new Assignment()
	//		{
	//			Day = new DateOnly(2024, 5, 3),
	//			Users = users.Where(u => u.Id == 3 || u.Id == 2).ToList()
	//		});
	//		assignments.Add(new Assignment()
	//		{
	//			Day = new DateOnly(2024, 5, 4),
	//			Users = users.Where(u => u.Id == 1 || u.Id == 2).ToList()
	//		});
	//		assignments.Add(new Assignment()
	//		{
	//			Day = new DateOnly(2024, 5, 5),
	//			Users = users.Where(u => u.Id == 3 || u.Id == 2).ToList()
	//		});

	//		//string method = "Any";
	//		//string properties = "Users";
	//		//string subMethod = "Count";
	//		//List<ExpressionType> subMethodOperations = new List<ExpressionType>();
	//		//subMethodOperations.Add(ExpressionType.LessThanOrEqual);
	//		//string methodValues = "0";
	//		//string subProp = "Id";
	//		//List<ExpressionType> subPropOperations = new List<ExpressionType>();
	//		//subPropOperations.Add(ExpressionType.Equal);
	//		//string subPropValues = "1";
	//		//ExpressionType operation = ExpressionType.LessThanOrEqual;
	//		//string values = "1";

	//		string method = "Count";
	//		string properties = "Day&Users";
	//		string subMethod = ",Any";
	//		List<ExpressionType> subMethodOperations = new List<ExpressionType>();
	//		string methodValues = ",0";
	//		string subProp = ",Id";
	//		List<ExpressionType> subPropOperations = new List<ExpressionType>();
	//		subPropOperations.Add(ExpressionType.Equal);
	//		subPropOperations.Add(ExpressionType.Equal);
	//		string subPropValues = "3/5/2024,2";
	//		ExpressionType operation = ExpressionType.Equal;
	//		string values = "1";

	//		List<Assignment> assignmentFiltered = FilterList<Assignment>.FilterQuery(assignments.AsQueryable(), properties, 
	//																				subMethod, subMethodOperations, methodValues, 
	//																				subProp, subPropOperations, subPropValues).ToList();
	//		bool conditionTrue = false;

	//		switch (method)
	//		{
	//			case "Count":
	//				switch (operation)
	//				{
	//					case ExpressionType.Equal:
	//						if(assignmentFiltered.Count == int.Parse(values))
	//						{
	//							conditionTrue = true;
	//						}
	//						break;
	//					case ExpressionType.LessThan:
	//						if (assignmentFiltered.Count < int.Parse(values))
	//						{
	//							conditionTrue = true;
	//						}
	//						break;
	//					case ExpressionType.GreaterThan:
	//						if (assignmentFiltered.Count > int.Parse(values))
	//						{
	//							conditionTrue = true;
	//						}
	//						break;
	//					case ExpressionType.LessThanOrEqual:
	//						if (assignmentFiltered.Count <= int.Parse(values))
	//						{
	//							conditionTrue = true;
	//						}
	//						break;
	//					default:
	//						if (assignmentFiltered.Count >= int.Parse(values))
	//						{
	//							conditionTrue = true;
	//						}
	//						break;
	//				}
	//				break;
	//			case "Any":
	//				if (assignmentFiltered.Count >= 1)
	//				{
	//					conditionTrue = true;
	//				}
	//				break;
	//			default:
	//				conditionTrue = true;
	//				break;
	//		}

	//		return Ok(conditionTrue);
	//		/*try
	//		{
	//			List<Unity> unities = _unityService.GetAllCommonUnities().Result;
	//			List<UnityModel> unitiesModel = new List<UnityModel>();
	//			foreach (Unity unity in unities)
	//			{
	//				unitiesModel.Add(new UnityModel(unity));
	//			}
	//			return Ok(unitiesModel);
	//		}
	//		catch (Exception ex)
	//		{
	//			return BadRequest(ex.Message);
	//		}*/
	//	}

	//	/*[HttpGet("{idUnity}")]
	//	public async Task<IActionResult> GetUnityById(int idUnity)
	//	{
	//		try
	//		{
	//			Unity unity = _unityService.GetUnityById(idUnity).Result;
	//			return Ok(unity == null || unity.Id == 0 ? null : new UnityModel(unity));
	//		}
	//		catch (Exception ex)
	//		{
	//			return BadRequest(ex.Message);
	//		}
	//	}

	//	// POST api/<LoginController>
	//	[HttpPost]
	//	public async Task<IActionResult> SaveUnity([FromBody] UnityModel unityModel)
	//	{
	//		try
	//		{
	//			bool result = true;
	//			if (unityModel.Id == 0)
	//			{
	//				result = _unityService.AddUnity(unityModel.Map()).Result;
	//			}
	//			else
	//			{
	//				result = _unityService.UpdateUnity(unityModel.Map()).Result;
	//			}
	//			return Ok(result);
	//		}
	//		catch (Exception ex)
	//		{
	//			return BadRequest(ex.Message);
	//		}
	//	}

	//	[HttpPost("{idCenter}")]
	//	public async Task<IActionResult> SaveUsers(int idCenter, [FromForm] IFormFile file)
	//	{
	//		try
	//		{
	//			List<Unity> newUnities = new List<Unity>();
	//			using (var reader = new StreamReader(file.OpenReadStream()))
	//			{
	//				while (reader.Peek() >= 0)
	//				{
	//					string? userStr = reader.ReadLine();
	//					string[] userArray = userStr == null ? [] : userStr.Split(';');
	//					if (userArray.Length == 2)
	//					{
	//						newUnities.Add(new Unity()
	//						{
	//							Name = userArray[0],
	//							Description = userArray[1]
	//						});
	//					}
	//					else
	//					{
	//						LogClass.WriteLog(ErrorWrite.Error, "Faltan o sobran datos ");
	//					}
	//				}

	//				bool result = await _unityService.AddUnities(newUnities);

	//				if (result)
	//				{
	//					return Ok(true);
	//				}
	//				else
	//				{
	//					return BadRequest("Se ha producido un error al guardar los usuarios.");
	//				}
	//			}
	//		}
	//		catch (Exception ex)
	//		{
	//			return BadRequest(ex.Message);
	//		}
	//	}

	//	// DELETE api/<LoginController>/5
	//	[HttpDelete("{id}")]
	//	public async Task<IActionResult> Delete(int id)
	//	{
	//		try
	//		{
	//			bool result = _unityService.DeleteUnity(id);
	//			return Ok(result);
	//		}
	//		catch (Exception ex)
	//		{
	//			return BadRequest(ex.Message);
	//		}
	//	}*/
	}
}
