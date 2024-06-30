using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class LevelRepository : ILevelRepository<Level>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public LevelRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<List<Level>> GetAllLevels()
		{
			List<Level> levels = new List<Level>();

			try
			{
				levels = await _context.Levels.ToListAsync();
				
				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado los niveles en la base de datos");

				return levels;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener los niveles. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<Level?> GetLevelByName(string name)
		{
			Level? level = new Level();

			try
			{
				level = await _context.Levels.Where(l => l.Name == name).FirstOrDefaultAsync();

				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado los niveles en la base de datos");

				return level;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener los niveles. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		#endregion
	}
}
