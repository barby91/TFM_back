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
    public class SpecialtyService : ISpecialtyService
    {
        #region variables
        private readonly ISpecialtyRepository<Specialty> _specialtyRepository;
        #endregion

        #region constructor
        public SpecialtyService(ISpecialtyRepository<Specialty> specialtyRepository)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia SpecialtyService");
			_specialtyRepository = specialtyRepository;
        }
		#endregion

		#region interface

		public async Task<List<SpecialtyModel>> GetAllSpecialtiesByCenter(int idCenter)
		{
			try
			{
				List<Specialty> specialties = await _specialtyRepository.GetAllSpecialtiesWithAllUnitiesByCenter(idCenter);

				List<SpecialtyModel> specialiesModel = new List<SpecialtyModel>();
				foreach (Specialty specialty in specialties)
				{
					specialiesModel.Add(new SpecialtyModel(specialty));
				}
				return await Task.FromResult(specialiesModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener las especialidades. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<SpecialtyModel?> GetSpecialtyById(int id)
		{
			try
			{
				Specialty? specialty = await _specialtyRepository.GetSpecialtyById(id);
				return await Task.FromResult((specialty == null || specialty.Id == 0) ? null : new  SpecialtyModel(specialty));
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener la especialidad de id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<SpecialtyModel?> GetSpecialtyByName(string name)
		{
			try
			{
				Specialty? specialty = await _specialtyRepository.GetSpecialtyByName(name);
				return await Task.FromResult(specialty == null ? null : new SpecialtyModel(specialty));
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener la especialidad de nombre {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), name, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddSpecialty(Specialty newSpecialty)
		{
			try
			{
				return await _specialtyRepository.AddSpecialty(newSpecialty);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al añadir una especialidad. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddSpecialties(List<SpecialtyModel> newSpecialties)
		{
			try
			{
				List<Specialty> specialties = new List<Specialty>();
				foreach(SpecialtyModel specialty in newSpecialties)
				{
					specialties.Add(specialty.Map());
				}
				return await _specialtyRepository.AddSpecialties(specialties);
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

		public async Task<bool> UpdateSpecialty(Specialty specialty)
		{
			try
			{
				return await _specialtyRepository.UpdateSpecialty(specialty);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al actaulizar la especialidad con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), specialty.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> DeleteSpecialty(int id)
		{
			try
			{
				return await _specialtyRepository.DeleteSpecialty(id);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al eliminar una especialidad. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		#endregion
	}
}
