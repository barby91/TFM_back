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
    public class LevelService : ILevelService
    {
        #region variables
        private readonly ILevelRepository<Level> _levelRepository;
        #endregion

        #region constructor
        public LevelService(ILevelRepository<Level> levelRepository)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia LevelService");
			_levelRepository = levelRepository;
        }
		#endregion

		#region interface

		public async Task<List<LevelModel>> GetAllLevels()
		{
			try
			{
				List<Level> levels = await _levelRepository.GetAllLevels();
				List<LevelModel> levelModel = new List<LevelModel>();
				foreach (Level level in levels)
				{
					levelModel.Add(new LevelModel(level));
				}
				return levelModel;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener los niveles. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<LevelModel?> GetLevelByName(string name)
		{
			try
			{
				Level? level = await _levelRepository.GetLevelByName(name);
				return level != null ? new LevelModel(level) : null;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener el nivel de nombre {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), name, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		#endregion
	}
}
