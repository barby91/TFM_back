using onGuardManager.Bussiness.IService;
using onGuardManager.Models;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.Service
{
    public class RolService : IRolService
    {
        #region variables
        private readonly IRolRepository<Rol> _rolRepository;
        #endregion

        #region constructor
        public RolService(IRolRepository<Rol> rolRepository)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia RolService");
			_rolRepository = rolRepository;
        }
		#endregion

		#region interface

		public async Task<List<RolModel>> GetAllRols()
		{
			try
			{
				List<Rol> rols = await _rolRepository.GetAllRols();
				List<RolModel> rolModel = new List<RolModel>();
				foreach (Rol rol in rols)
				{
					rolModel.Add(new RolModel(rol));
				}
				return await Task.FromResult(rolModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener los roles. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<RolModel?> GetRolByName(string name)
		{
			try
			{
				Rol? rol = await _rolRepository.GetRolByName(name);
				RolModel? rolModel = rol == null ? null : new RolModel(rol);
				return await Task.FromResult(rolModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener el rol d enombre {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), name, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		#endregion
	}
}
