using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace onGuardManager.Test.Repository
{
	public class AskedHolidayRepositoryTest
	{
		private IAskedHolidayRepository<AskedHoliday> _askedHolidayRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<AskedHoliday>>(x => x.AskedHolidays)
				.ReturnsDbSet(GetFakeAskedHoliday());
			_askedHolidayRepository = new AskedHolidayRepository(dbContext.Object);
		}

		[TestCaseSource(nameof(GetPendingHolidayCase))]
		[Test]
		public void AskedHolidayRepositoryTestGetAllPendingAskedHolidaysByCenter(int idCenter, int idUser, string type, List<AskedHoliday> expected)
		{
			#region Actual
			List<AskedHoliday> actual = _askedHolidayRepository.GetAllPendingAskedHolidaysByCenter(idCenter, idUser, type).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].DateTo, Is.EqualTo(expected[i].DateTo));
				Assert.That(actual[i].DateFrom, Is.EqualTo(expected[i].DateFrom));
				Assert.That(actual[i].IdStatus, Is.EqualTo(expected[i].IdStatus));
				Assert.That(actual[i].IdStatusNavigation.Description, Is.EqualTo(expected[i].IdStatusNavigation.Description));
				Assert.That(actual[i].IdUser, Is.EqualTo(expected[i].IdUser));
				Assert.That(actual[i].IdUserNavigation.IdCenter, Is.EqualTo(expected[i].IdUserNavigation.IdCenter));
			}
			#endregion
		}

		[Test]
		public void AskedHolidayRepositoryTestGetAllPendingAskedHolidaysByCenterException()
		{
			#region Arrange
			dbContext.Setup(x => x.AskedHolidays).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _askedHolidayRepository.GetAllPendingAskedHolidaysByCenter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
		}

		[TestCaseSource(nameof(GetAddAskedHolidayCase))]
		[Test]
		public void AskedHolidayRepositoryTestAddAskedHoliday(AskedHoliday askedHoliday, bool expected, int expectedAddAskedHoliday, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int addAskedHoliday = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.AskedHolidays.AddAsync(It.IsAny<AskedHoliday>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addAskedHoliday = ++callCount);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_askedHolidayRepository.AddAskedHoliday(askedHoliday);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.AskedHolidays.AddAsync(It.IsAny<AskedHoliday>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addAskedHoliday, Is.EqualTo(expectedAddAskedHoliday));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void AskedHolidayRepositoryTestAddAskedHolidayException()
		{	
			Assert.ThrowsAsync<NullReferenceException>(async() => await _askedHolidayRepository.AddAskedHoliday(It.IsAny<AskedHoliday>()));
		}

		[TestCaseSource(nameof(GetUpdateAskedHolidayCase))]
		[Test]
		public void AskedHolidayRepositoryTestUpdateAskedHoliday(AskedHoliday askedHoliday, bool expected, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => saveChanges = callCount++);
			#endregion

			_askedHolidayRepository.UpdateAskedHoliday(askedHoliday);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.SaveChangesAsync(default), Times.Once());
			}

			#region Assert
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void AskedHolidayRepositoryTestUpdateAskedHolidayException()
		{
			#region Arrange		
			AskedHoliday askedHoliday = new AskedHoliday
			{
				DateFrom = new DateOnly(2024, 5, 5),
				DateTo = new DateOnly(2024, 5, 15),
				Id = 1,
				IdStatus = 1,
				IdStatusNavigation = new HolidayStatus
				{
					Description = "Cancelado"
				},
				IdUser = 1,
				IdUserNavigation = new User()
				{
					IdCenter = 1
				},
				Period = "2024"
			};

			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _askedHolidayRepository.UpdateAskedHoliday(askedHoliday));
		}

		[Test]
		public void AskedHolidayRepositoryTestGetPendingAskedHolidaysByDates()
		{
			#region Arrange

			AskedHoliday expected = new AskedHoliday
			{
				DateFrom = new DateOnly(2024, 5, 5),
				DateTo = new DateOnly(2024, 5, 15),
				Id = 1,
				IdStatus = 1,
				IdStatusNavigation = new HolidayStatus
				{
					Description = "Solicitado"
				},
				IdUser = 1,
				IdUserNavigation = new User()
				{
					IdCenter = 1
				},
				Period = "2024"
			};

			#endregion
			#region Actual
			AskedHoliday? actual = _askedHolidayRepository.GetPendingAskedHolidaysByDates(new DateOnly(2024, 5, 5), new DateOnly(2024, 5, 15), 1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.DateTo, Is.EqualTo(expected.DateTo));
			Assert.That(actual.DateFrom, Is.EqualTo(expected.DateFrom));
			Assert.That(actual.IdStatus, Is.EqualTo(expected.IdStatus));
			Assert.That(actual.IdStatusNavigation.Description, Is.EqualTo(expected.IdStatusNavigation.Description));
			Assert.That(actual.IdUser, Is.EqualTo(expected.IdUser));
			Assert.That(actual.IdUserNavigation.IdCenter, Is.EqualTo(expected.IdUserNavigation.IdCenter));
			#endregion
		}

		[Test]
		public void AskedHolidayRepositoryTestGetPendingAskedHolidaysByDatesException()
		{
			#region Arrange
			dbContext.Setup(x => x.AskedHolidays).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _askedHolidayRepository.GetPendingAskedHolidaysByDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()));
		}


		private static object[] GetPendingHolidayCase =
		{
			new object[] {1, 2, "Solicitado", new List<AskedHoliday>() {
					new AskedHoliday
					{
						DateFrom = new DateOnly(2024, 5, 5),
						DateTo = new DateOnly(2024, 5, 15),
						Id = 1,
						IdStatus = 1,
						IdStatusNavigation = new HolidayStatus
						{
							Description = "Solicitado"
						},
						IdUser = 1,
						IdUserNavigation = new User()
						{
							IdCenter = 1
						},
						Period = "2024"
					}
				}
			},
			new object[] { 1, 2, "Cancelado", new List<AskedHoliday>() {
					new AskedHoliday
					{
						DateFrom = new DateOnly(2024, 5, 5),
						DateTo = new DateOnly(2024, 5, 15),
						Id = 3,
						IdStatus = 3,
						IdStatusNavigation = new HolidayStatus
						{
							Description = "Cancelado"
						},
						IdUser = 1,
						IdUserNavigation = new User()
						{
							IdCenter = 1
						},
						Period = "2024"
					}
				}
			},
			new object[]
			{
				1, 2, "Aprobado", new List<AskedHoliday>()
				{
					new AskedHoliday
					{
						DateFrom = new DateOnly(2024, 5, 5),
						DateTo = new DateOnly(2024, 5, 15),
						Id = 5,
						IdStatus = 2,
						IdStatusNavigation = new HolidayStatus
						{
							Description = "Aprobado"
						},
						IdUser = 1,
						IdUserNavigation = new User()
						{
							IdCenter = 1
						},
						Period = "2024"
					}
				} 
			},
			new object[]
			{
				5, 2, "Aprobado", new List<AskedHoliday>()
			}
		};

		private static object[] GetAddAskedHolidayCase =
		{
			new object[] { new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 10, 5),
					DateTo = new DateOnly(2024, 10, 15),
					IdStatus = 1,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Solicitado"
					},
					IdUser = 1,
					IdUserNavigation = new User()
					{
						IdCenter = 1
					},
					Period = "2024"
				}, true, 1, 1},
			new object[] { new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					IdStatus = 1,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Solicitado"
					},
					IdUser = 1,
					IdUserNavigation = new User()
					{
						IdCenter = 1
					},
					Period = "2024"
				}, false, 0, 0 }
		};

		private static object[] GetUpdateAskedHolidayCase =
		{
			new object[] { 
				new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					IdStatus = 1,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Cancelado"
					},
					IdUser = 1,
					IdUserNavigation = new User()
					{
						IdCenter = 1
					},
					Period = "2024"
				}, true, 0
			},
			new object[] { new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 8,
					IdStatus = 1,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Cancelado"
					},
					IdUser = 1,
					IdUserNavigation = new User()
					{
						IdCenter = 1
					},
					Period = "2024"
				}, false, 0
			}
		};

		private List<AskedHoliday> GetFakeAskedHoliday()
		{
			return new List<AskedHoliday>()
			{
				new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					IdStatus = 1,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Solicitado"
					},
					IdUser = 1,
					IdUserNavigation = new User()
					{
						IdCenter = 1
					},
					Period = "2024"
				},
				new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 2,
					IdStatus = 1,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Solicitado"
					},
					IdUser = 2,
					IdUserNavigation = new User()
					{
						IdCenter = 2
					},
					Period = "2024"
				},
				new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 3,
					IdStatus = 3,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Cancelado"
					},
					IdUser = 1,
					IdUserNavigation = new User()
					{
						IdCenter = 1
					},
					Period = "2024"
				},
				new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 4,
					IdStatus = 3,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Cancelado"
					},
					IdUser = 2,
					IdUserNavigation = new User()
					{
						IdCenter = 2
					},
					Period = "2024"
				},
				new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 5,
					IdStatus = 2,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Aprobado"
					},
					IdUser = 1,
					IdUserNavigation = new User()
					{
						IdCenter = 1
					},
					Period = "2024"
				},
				new AskedHoliday
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 6,
					IdStatus = 2,
					IdStatusNavigation = new HolidayStatus
					{
						Description = "Aprobado"
					},
					IdUser = 2,
					IdUserNavigation = new User()
					{
						IdCenter = 2
					},
					Period = "2024"
				}
			};
		}

	}
}
