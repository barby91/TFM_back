using onGuardManager.Bussiness.IService;
using onGuardManager.Models;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;

namespace onGuardManager.Bussiness.Service
{
 //   public class DayGuardUserService : IDayGuardUserService
	//{
 //       #region variables
 //       private readonly IDayGuardUserRepository<DayGuardUser> _dayGuardUserRepository;
 //       #endregion

 //       #region constructor
 //       public DayGuardUserService(IDayGuardUserRepository<DayGuardUser> dayGuadUserRepository)
 //       {
 //           LogClass.WriteLog(ErrorWrite.Info, "se inicia DayGuadUserService");
	//		_dayGuardUserRepository = dayGuadUserRepository;
 //       }
	//	#endregion

	//	#region interface

	//	public async Task<bool> SaveGuard(List<DayGuardUser> newGuardDay)
	//	{
	//		try
	//		{
	//			return await _dayGuardUserRepository.SaveGuard(newGuardDay);
	//		}
	//		catch (Exception ex)
	//		{
	//			StringBuilder sb = new StringBuilder("");
	//			sb.AppendFormat(" Se ha producido un error al guardar las asignaciones de la guardia." +
	//							"La traza es: {0}: ", ex.ToString());
	//			LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
	//			throw;
	//		}
	//	}

	//	public async Task<List<DayGuardUser>> GetGuards(int year, int month)
	//	{
	//		try
	//		{
	//			return await _dayGuardUserRepository.GetGuards(year, month);
	//		}
	//		catch (Exception ex)
	//		{
	//			StringBuilder sb = new StringBuilder("");
	//			sb.AppendFormat(" Se ha producido un error obtener las guardias del mes {0}." +
	//							"La traza es: {1}: ", month, ex.ToString());
	//			LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
	//			throw;
	//		}
	//	}

	//	#endregion
	//}
}
