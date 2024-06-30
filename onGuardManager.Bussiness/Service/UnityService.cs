using onGuardManager.Bussiness.IService;
using onGuardManager.Models;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.Service
{
    public class UnityService : IUnityService
    {
        #region variables
        private readonly IUnityRepository<Unity> _unityRepository;
        #endregion

        #region constructor
        public UnityService(IUnityRepository<Unity> unityRepository)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia UnityService");
			_unityRepository = unityRepository;
        }
		#endregion

		#region interface

		public async Task<List<UnityModel>> GetAllCommonUnities()
		{
			try
			{
				List<Unity> unities = await _unityRepository.GetAllCommonUnities();
				List<UnityModel> unitiesModel = new List<UnityModel>();
				foreach (Unity unity in unities)
				{
					unitiesModel.Add(new UnityModel(unity));
				}
				return await Task.FromResult(unitiesModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener las unidades comunes. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<UnityModel?> GetUnityById(int id)
		{
			try
			{
				Unity? unity = await _unityRepository.GetUnityById(id);
				UnityModel? unityModel = (unity == null || unity.Id == 0) ? null : new UnityModel(unity);
				return await Task.FromResult(unityModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener la unidad de id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<UnityModel?> GetUnityByName(string name)
		{
			try
			{
				Unity? unity = await _unityRepository.GetUnityByName(name);
				UnityModel? unityModel = (unity == null || unity.Id == 0) ? null : new UnityModel(unity);
				return await Task.FromResult(unityModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener la unidad de nombre {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), name, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUnity(Unity newUnity)
		{
			try
			{
				return await _unityRepository.AddUnity(newUnity);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al añadir una unidad. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUnities(StreamReader reader)
		{ 
			try
			{
				List<Unity> newUnities = new List<Unity>();
				string? unityStr = reader.ReadLine();
				while (unityStr != null)
				{
					string[] unityArray = unityStr == String.Empty ? [] : unityStr.Split(';');
					if (unityArray.Length == 2)
					{
						newUnities.Add(new Unity()
						{
							Name = unityArray[0],
							Description = unityArray[1]
						});
					}
					else
					{
						LogClass.WriteLog(ErrorWrite.Error, "Faltan o sobran datos ");
					}
					unityStr = reader.ReadLine();
				}

				bool result = await _unityRepository.AddUnities(newUnities);

				return await Task.FromResult(result);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al añadir las especialidades. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> UpdateUnity(Unity unity)
		{
			try
			{
				return await _unityRepository.UpdateUnity(unity);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al actaulizar la unidad con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), unity.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> DeleteUnity(int id)
		{
			try
			{
				return await _unityRepository.DeleteUnity(id);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al eliminar una unidad. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		#endregion
	}
}
