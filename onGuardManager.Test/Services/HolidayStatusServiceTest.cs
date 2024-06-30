using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Services
{
	public class HolidayStatusServiceTest
	{
		private Mock<IHolidayStatusRepository<HolidayStatus>> _holidayStatusRepository;
		private IHolidayStatusService _serviceHolidayStatus;

		[SetUp]
		public void Setup()
		{
			_holidayStatusRepository = new Mock<IHolidayStatusRepository<HolidayStatus>>();
			_serviceHolidayStatus = new HolidayStatusService(_holidayStatusRepository.Object);
		}

		[TestCaseSource(nameof(GetDescriptions))]
		[Test]
		public void HolidayStatusServiceTestGetAllHolidayStatus(string description, HolidayStatus expected)
		{
			#region Arrange
			_holidayStatusRepository.Setup(ur => ur.GetIdHolidayStatusByDescription(description)).ReturnsAsync((int)expected.Id);
			#endregion

			#region Actual
			int actual = _serviceHolidayStatus.GetIdHolidayStatusByDescription(description).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual, Is.EqualTo(expected.Id));
			#endregion
		}

		[Test]
		public void HolidayStatusServiceTestGetAllHolidayStatusException()
		{
			#region Arrange
			_holidayStatusRepository.Setup(ur => ur.GetIdHolidayStatusByDescription(It.IsAny<string>())).Throws(() => new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _serviceHolidayStatus.GetIdHolidayStatusByDescription("descripcion"));
		}

		private static object[] GetDescriptions =
		{
			new object[] 
			{
				"Cancelado", new HolidayStatus
							{
								Id = 1,
								Description = "Cancelado"
							}
			},
			new object[] 
			{
				"Aprobado", new HolidayStatus
							{
								Id = 2,
								Description = "Aprobado"
							}
			},
			new object[]
			{
				"Solicitado", new HolidayStatus
								{
									Id = 3,
									Description = "Solicitado"
								}
			},
			new object[]
			{
				"Aleatorio", new HolidayStatus
								{
									Id = 0,
									Description = "Aleatorio"
								}
			}
		};
	}
}
