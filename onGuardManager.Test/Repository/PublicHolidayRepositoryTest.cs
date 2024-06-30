using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Repository
{
	public class PublicHolidayRepositoryTest
	{
		private IPublicHolidayRepository<PublicHoliday> _publicHolidayRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<PublicHolidayCenter>>(x => x.PublicHolidayCenters)
				.ReturnsDbSet(GetFakePublicHolidays());
			_publicHolidayRepository = new PublicHolidayRepository(dbContext.Object);
		}

		[TestCaseSource(nameof(GetPublicHolidaysCase))]
		[Test]
		public void PublicHolidayRepositoryTestGetAllPublicHolidaysByCenter(int idCenter, List<PublicHoliday> expected)
		{
			#region Actual
			List<PublicHoliday> actual = _publicHolidayRepository.GetAllPublicHolidaysByCenter(idCenter).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].IdType, Is.EqualTo(expected[i].IdType));
				Assert.That(actual[i].Date, Is.EqualTo(expected[i].Date));
				Assert.That(actual[i].IdTypeNavigation.Id, Is.EqualTo(expected[i].IdTypeNavigation.Id));
				Assert.That(actual[i].IdTypeNavigation.Description, Is.EqualTo(expected[i].IdTypeNavigation.Description));
				CollectionAssert.AreEqual(actual[i].IdTypeNavigation.PublicHolidays, expected[i].IdTypeNavigation.PublicHolidays);
			}
			#endregion
		}

		[Test]
		public void PublicHolidayRepositoryTestGetAllPublicHolidaysByCenterException()
		{
			#region Arrange
			dbContext.Setup(x => x.PublicHolidayCenters).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _publicHolidayRepository.GetAllPublicHolidaysByCenter(It.IsAny<int>()));
		}

		private static object[] GetPublicHolidaysCase =
		{
			new object[] { 1, new List<PublicHoliday>() {
					new PublicHoliday{
						Id = 1,
						Date = new DateOnly(2024, 6, 1),
						IdType = 1,
						IdTypeNavigation = new PublicHolidayType
						{
							Description = "Nacional",
							Id = 1
						}
					},
					new PublicHoliday{
						Id = 2,
						Date = new DateOnly(2024, 5, 2),
						IdType = 2,
						IdTypeNavigation = new PublicHolidayType
						{
							Description = "Regional",
							Id = 2
						}
					},
					new PublicHoliday{
						Id = 3,
						Date = new DateOnly(2024, 7, 25),
						IdType = 3,
						IdTypeNavigation = new PublicHolidayType()
						{
							Description = "Local",
							Id = 3
						}
					}
			}},
			new object[] { 8, new List<PublicHoliday>()}
		};


		private List<PublicHolidayCenter> GetFakePublicHolidays()
		{
			return new List<PublicHolidayCenter>()
			{
				new PublicHolidayCenter
				{
					IdCenter = 1,
					IdCenterNavigation = new Center()
					{
						Id = 1,
						City = "Ciudad",
						Name = "Nombre"
					},
					IdPublicHoliday = 1,
					IdPublicHolidayNavigation = new PublicHoliday{
						Id = 1,
						Date = new DateOnly(2024, 6, 1),
						IdType = 1,
						IdTypeNavigation = new PublicHolidayType
						{
							Description = "Nacional",
							Id = 1
						}
					}
				},
				new PublicHolidayCenter
				{
					IdCenter = 1,
					IdCenterNavigation = new Center()
					{
						Id = 1,
						City = "Ciudad",
						Name = "Nombre"
					},
					IdPublicHoliday = 2,
					IdPublicHolidayNavigation = new PublicHoliday{
						Id = 2,
						Date = new DateOnly(2024, 5, 2),
						IdType = 2,
						IdTypeNavigation = new PublicHolidayType
						{
							Description = "Regional",
							Id = 2
						}
					}
				},
				new PublicHolidayCenter
				{
					IdCenter = 1,
					IdCenterNavigation = new Center()
					{
						Id = 1,
						City = "Ciudad",
						Name = "Nombre"
					},
					IdPublicHoliday = 3,
					IdPublicHolidayNavigation = new PublicHoliday{
						Id = 3,
						Date = new DateOnly(2024, 7, 25),
						IdType = 3,
						IdTypeNavigation = new PublicHolidayType()
						{
							Description = "Local",
							Id = 3
						}
					}
				},
				new PublicHolidayCenter
				{
					IdCenter = 2,
					IdCenterNavigation = new Center()
					{
						Id = 2,
						City = "Ciudad2",
						Name = "Nombre2"
					},
					IdPublicHoliday = 4,
					IdPublicHolidayNavigation = new PublicHoliday{
						Id = 4,
						Date = new DateOnly(2024, 6, 1),
						IdType = 1,
						IdTypeNavigation = new PublicHolidayType
						{
							Description = "Nacional",
							Id = 1
						}
					}
				},
				new PublicHolidayCenter
				{
					IdCenter = 2,
					IdCenterNavigation = new Center()
					{
						Id = 2,
						City = "Ciudad",
						Name = "Nombre"
					},
					IdPublicHoliday = 5,
					IdPublicHolidayNavigation = new PublicHoliday{
						Id = 5,
						Date = new DateOnly(2024, 5, 2),
						IdType = 2,
						IdTypeNavigation = new PublicHolidayType
						{
							Description = "Regional",
							Id = 2
						}
					}
				},
				new PublicHolidayCenter
				{
					IdCenter = 2,
					IdCenterNavigation = new Center()
					{
						Id = 2,
						City = "Ciudad2",
						Name = "Nombre2"
					},
					IdPublicHoliday = 6,
					IdPublicHolidayNavigation = new PublicHoliday{
						Id = 6,
						Date = new DateOnly(2024, 7, 25),
						IdType = 3,
						IdTypeNavigation = new PublicHolidayType()
						{
							Description = "Local",
							Id = 3
						}
					}
				}
			};
		}
	}
}
