using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class RolRepository : IRolRepository<Rol>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public RolRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<List<Rol>> GetAllRols()
		{
			List<Rol> rols = new List<Rol>();

			try
			{
				rols = await _context.Rols.ToListAsync();
				
				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado los roles en la base de datos");

				return rols;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener los roles. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<Rol?> GetRolByName(string name)
		{
			Rol? rol = new Rol();

			try
			{
				rol = await _context.Rols.Where(l => l.Name == name).FirstOrDefaultAsync();

				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado los niveles en la base de datos");

				return rol;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener el rol de nombre{2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), name, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		#endregion
	}
}
