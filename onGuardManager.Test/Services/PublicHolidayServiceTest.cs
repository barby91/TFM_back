using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Services
{
	public class PublicHolidayServiceTest
	{
		private Mock<IPublicHolidayRepository<PublicHoliday>> _publicHolidayRepository;
		private IPublicHolidayService _publicHolidayStatus;

		[SetUp]
		public void Setup()
		{
			_publicHolidayRepository = new Mock<IPublicHolidayRepository<PublicHoliday>>();
			_publicHolidayStatus = new PublicHolidayService(_publicHolidayRepository.Object);
		}

		[TestCaseSource(nameof(GetPublicHolidaysCase))]
		[Test]
		public void PublicHolidayServiceTestGetAllLevels(int idCenter, List<PublicHolidayModel> expected, List<PublicHoliday> resultItem)
		{
			#region Arrange
			_publicHolidayRepository.Setup(ur => ur.GetAllPublicHolidaysByCenter(idCenter)).ReturnsAsync(resultItem);
			#endregion

			#region Actual
			List<PublicHolidayModel> actual = _publicHolidayStatus.GetAllPublicHolidaysByCenter(idCenter).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].TypeLabel, Is.EqualTo(expected[i].TypeLabel));
				Assert.That(actual[i].Date, Is.EqualTo(expected[i].Date));
			}
			#endregion
		}

		[Test]
		public void PublicHolidayServiceTestGetAllLevelsException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _publicHolidayStatus.GetAllPublicHolidaysByCenter(It.IsAny<int>()));
		}


		private static object[] GetPublicHolidaysCase =
		{
			new object[] 
			{ 
				1, 
				new List<PublicHolidayModel>() 
				{
					new PublicHolidayModel{
						Id = 1,
						Date = new DateOnly(2024, 6, 1),
						TypeLabel = "Nacional",
					},
					new PublicHolidayModel{
						Id = 2,
						Date = new DateOnly(2024, 5, 2),
						TypeLabel = "Regional"
					},
					new PublicHolidayModel{
						Id = 3,
						Date = new DateOnly(2024, 7, 25),
						TypeLabel = "Local"
					}
				}, 
				new List<PublicHoliday>() 
				{
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
				}
			},
			new object[] { 8, new List<PublicHolidayModel>(), new List<PublicHoliday>() }
		};
	}
}
