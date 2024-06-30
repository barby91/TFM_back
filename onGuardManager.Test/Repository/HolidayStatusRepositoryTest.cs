using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Repository
{
	public class HolidayStatusRepositoryTest
	{
		private IHolidayStatusRepository<HolidayStatus> _holidayStatusRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<HolidayStatus>>(x => x.HolidayStatuses)
				.ReturnsDbSet(GetFakeHolidayStatuses());
			_holidayStatusRepository = new HolidayStatusRepository(dbContext.Object);
		}

		[TestCaseSource(nameof(GetDescriptions))]
		[Test]
		public void HolidayStatusRepositoryTestGetAllHolidayStatus(string description, HolidayStatus expected)
		{
			#region Actual
			int actual = _holidayStatusRepository.GetIdHolidayStatusByDescription(description).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual, Is.EqualTo(expected.Id));
			#endregion
		}

		[Test]
		public void HolidayStatusRepositoryTestGetAllHolidayStatusException()
		{
			#region Arrange
			dbContext.Setup(x => x.HolidayStatuses).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _holidayStatusRepository.GetIdHolidayStatusByDescription(It.IsAny<string>()));
		}

		private List<HolidayStatus> GetFakeHolidayStatuses()
		{
			return new List<HolidayStatus>()
			{
				new HolidayStatus
				{
					Id = 1,
					Description = "Cancelado"
				},
				new HolidayStatus
				{
					Id = 2,
					Description = "Aprobado"
				},
				new HolidayStatus
				{
					Id = 3,
					Description = "Solicitado"
				}
			};
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
