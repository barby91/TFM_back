using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Entities;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;
using Assert = NUnit.Framework.Assert;

namespace onGuardManager.Test.Repository
{
	public class DayGuardServiceTest
	{
		private Mock<IDayGuardRepository<DayGuard>> _dayGuardRepository;
		private Mock<IUserRepository<User>> _userRepository;
		private Mock<IPublicHolidayRepository<PublicHoliday>> _publicHolidayRepository;
		private Mock<ISpecialtyRepository<Specialty>> _specialtyRepository;
		private Mock<IUnityRepository<Unity>> _unityRepository;
		private IDayGuardService _serviceDayGuard;

		[SetUp]
		public void Setup()
		{
			_dayGuardRepository = new Mock<IDayGuardRepository<DayGuard>>();
			_userRepository = new Mock<IUserRepository<User>>();
			_publicHolidayRepository = new Mock<IPublicHolidayRepository<PublicHoliday>>();
			_specialtyRepository = new Mock<ISpecialtyRepository<Specialty>>();
			_unityRepository = new Mock<IUnityRepository<Unity>>();
			_serviceDayGuard = new DayGuardService(_dayGuardRepository.Object,
													_userRepository.Object,
													_publicHolidayRepository.Object,
													_specialtyRepository.Object,
													_unityRepository.Object);
		}

		[TestCaseSource(nameof(GetAddGuardsCase))]
		[Test]
		public void DayGuardServiceTestSaveGuard(DayGuard dayGuard, bool expected)
		{
			#region Arrange
			_dayGuardRepository.Setup(x => x.SaveGuard(It.IsAny<DayGuard>())).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceDayGuard.SaveGuard(dayGuard).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}
		
		[Test]
		public void DayGuardServiceTestSaveGuardException()
		{
			#region Arrange
			_dayGuardRepository.Setup(ur => ur.SaveGuard(It.IsAny<DayGuard>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceDayGuard.SaveGuard(new DayGuard
																							{
																								Day = new DateOnly(2024, 01, 02),
																								assignedUsers = new List<User>()
																													{
																														new User()
																														{
																															Id = 1,
																															Name = "usuario1",
																															IdCenter = 1,
																															IdLevel = 1,
																															IdSpecialty = 1,
																															Surname = "usuario"
																														}
																													}
																							}));
		}
		
		[TestCaseSource(nameof(GetDeleteGuardsCase))]
		[Test]
		//public void DayGuardServiceTestDeletePreviousGuard(GuardInterval guardInterval, bool expected)
		//{
		//	#region Arrange
		//	_dayGuardRepository.Setup(ur => ur.DeletePreviousGuard(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(expected);
		//	#endregion

		//	#region Actual

		//	bool actual = _serviceDayGuard.DeletePreviousGuard(guardInterval).Result;
		//	#endregion

		//	#region Assert
		//	Assert.That(actual, Is.EqualTo(expected));
		//	#endregion
		//}

		public void DayGuardServiceTestDeletePreviousGuard(int month, bool expected)
		{
			#region Arrange
			_dayGuardRepository.Setup(ur => ur.DeletePreviousGuard(It.IsAny<int>())).ReturnsAsync(expected);
			#endregion

			#region Actual

			bool actual = _serviceDayGuard.DeletePreviousGuard(month).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		//public void DayGuardServiceTestGetlDeletePreviousGuardException()
		//{
		//	#region Arrange
		//	_dayGuardRepository.Setup(ur => ur.DeletePreviousGuard(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Throws(() => new Exception());
		//	#endregion

		//	Assert.ThrowsAsync<Exception>(async () => await _serviceDayGuard.DeletePreviousGuard(new GuardInterval()
		//	{
		//		firstDayInterval = It.IsAny<DateOnly>(),
		//		lastDayInterval = It.IsAny<DateOnly>()
		//	}
		//	));
		//}

		public void DayGuardServiceTestGetlDeletePreviousGuardException()
		{
			#region Arrange
			_dayGuardRepository.Setup(ur => ur.DeletePreviousGuard(It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async () => await _serviceDayGuard.DeletePreviousGuard(It.IsAny<int>()));
		}

		[TestCaseSource(nameof(GetUserStatsCase))]
		[Test]
		public void DayGuardServiceTestGetUserStats(GuardRequest request, string expected, List<User> fakeUsers, bool SaveGuardReturn, bool DeleteGuardReturn)
		{
			#region Arrange
			_publicHolidayRepository.Setup(ph => ph.GetAllPublicHolidaysByCenter(It.IsAny<int>())).ReturnsAsync(GetFakePublicHolidays());
			_userRepository.Setup(u => u.GetAllUsersByCenter(It.IsAny<int>(), true)).ReturnsAsync(fakeUsers);
			_userRepository.Setup(u => u.GetAllUsersBySpecialty(1)).ReturnsAsync(fakeUsers.Where(u => u.IdSpecialty == 1).ToList());
			_userRepository.Setup(u => u.GetAllUsersBySpecialty(2)).ReturnsAsync(fakeUsers.Where(u => u.IdSpecialty == 2).ToList());
			_specialtyRepository.Setup(s => s.GetAllSpecialtiesWithoutCommonUnitiesByCenter(It.IsAny<int>())).ReturnsAsync(GetFakeSpecialties());
			_specialtyRepository.Setup(s => s.GetSpecialtyById(It.IsAny<int>())).ReturnsAsync(GetFakeSpecialties()[0]);
			_unityRepository.Setup(u => u.GetAllCommonUnities(It.IsAny<int>())).ReturnsAsync(GetFakeCommonUnities());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2023, It.IsAny<int>())).ReturnsAsync(new List<DayGuard>());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 0)).ReturnsAsync(GetFakeDayGuards());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 1)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 1).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 2)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 2).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 3)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 3).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 4)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 4).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 5)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 5).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 6)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 6).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 7)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 7).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 8)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 8).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 9)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 9).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 10)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 10).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 11)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 11).ToList());
			_dayGuardRepository.Setup(dg => dg.GetGuards(It.IsAny<int>(), 2024, 12)).ReturnsAsync(GetFakeDayGuards().Where(dg => dg.Day.Month == 12).ToList());
			//_dayGuardRepository.Setup(dg => dg.DeletePreviousGuard(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(DeleteGuardReturn);
			_dayGuardRepository.Setup(dg => dg.DeletePreviousGuard(It.IsAny<int>())).ReturnsAsync(DeleteGuardReturn);
			_dayGuardRepository.Setup(dg => dg.SaveGuard(It.IsAny<DayGuard>())).ReturnsAsync(SaveGuardReturn);
			#endregion

			#region Actual
			string actual = _serviceDayGuard.GetUserStats(request).Result;
			#endregion

			#region Assert
			Assert.That(actual.Contains(expected), Is.EqualTo(true));
			#endregion
		}

		[TestCaseSource(nameof(GetGuardsCase))]
		[Test]
		public void DayGuardServiceTestGetGuards(List<DayGuard> returnObject, List<DayGuardModel> expected, int center, int year, int month)
		{

			#region Arrange
			_dayGuardRepository.Setup(ur => ur.GetGuards(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(returnObject);
			#endregion

			#region Actual
			List<DayGuardModel> actual = _serviceDayGuard.GetGuards(center, year, month).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Day, Is.EqualTo(expected[i].Day));
				Assert.That(actual[i].assignedUsers.Count, Is.EqualTo(expected[i].assignedUsers.Count));
				for(int j = 0; j < actual[i].assignedUsers.Count; j++)
				{
					Assert.That(actual[i].assignedUsers.ToList()[j].NameSurname, Is.EqualTo(expected[i].assignedUsers.ToList()[j].NameSurname));
				}
			}
			#endregion
		}
		
		[Test]
		public void DayGuardServiceTestGetGuardsException()
		{
			#region Arrange
			_dayGuardRepository.Setup(ur => ur.GetGuards(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async () => await _serviceDayGuard.GetGuards(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));
		}

		private List<PublicHoliday> GetFakePublicHolidays()
		{
			return new List<PublicHoliday>()
			{
				new PublicHoliday{ 
					Id = 1, 
					Date = new DateOnly(2024, 1, 1), 
					IdType=1},
				new PublicHoliday{ 
					Id = 2, 
					Date = new DateOnly(2024, 1, 6),
					IdType=1},
				new PublicHoliday{ 
					Id = 3, 
					Date = new DateOnly(2024, 3, 29), 
					IdType=1},
				new PublicHoliday{ 
					Id = 4, 
					Date = new DateOnly(2024, 5, 1), 
					IdType=1},
				new PublicHoliday{ 
					Id = 5,
					Date = new DateOnly(2024, 8, 15), 
					IdType=1},
				new PublicHoliday{ 
					Id = 6, 
					Date = new DateOnly(2024, 10, 12), 
					IdType=1},
				new PublicHoliday{ 
					Id = 7, 
					Date = new DateOnly(2024, 11, 1), 
					IdType=1},
				new PublicHoliday{ 
					Id = 8, 
					Date = new DateOnly(2024, 12, 6), 
					IdType=1},
				new PublicHoliday{ 
					Id = 9, 
					Date = new DateOnly(2024, 12, 25), 
					IdType=1},
				new PublicHoliday{ 
					Id = 10, 
					Date = new DateOnly(2024, 3, 28),
					IdType=2},
				new PublicHoliday{ 
					Id = 11, 
					Date = new DateOnly(2024, 5, 2), 
					IdType=2},
				new PublicHoliday{ 
					Id = 12, 
					Date = new DateOnly(2024, 7, 25),
					IdType=2},
				new PublicHoliday{ 
					Id = 13, 
					Date = new DateOnly(2024, 11, 9), 
					IdType=3},
				new PublicHoliday{ 
					Id = 14, 
					Date = new DateOnly(2024, 5, 15), 
					IdType=3}

			};
		}

		private static List<User> GetFakeUserWithHolidays()
		{
			return new List<User>()
			{
				new User{Id=60142, Name="usuario", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60143, Name="usuario2", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60144, Name="usuario3", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60145, Name="usuario4", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60146, Name="usuario5", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60147, Name="usuario6", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60148, Name="usuario7", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60149, Name="usuario8", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60150, Name="usuario9", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60151, Name="usuario10", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60152, Name="usuario11", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60153, Name="usuario12", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60154, Name="usuario13", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60155, Name="usuario14", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60156, Name="usuario15", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60157, Name="usuario16", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60158, Name="usuario17", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60159, Name="usuario18", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60160, Name="usuario19", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60161, Name="usuario20", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60162, Name="usuario21", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60163, Name="usuario22", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60164, Name="usuario23", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60165, Name="usuario24", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60166, Name="usuario25", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60167, Name="usuario26", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60168, Name="usuario27", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60169, Name="usuario28", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60170, Name="usuario29", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60171, Name="usuario30", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
			};
		}

		private static List<User> GetFakeUserWithHolidaysSpecialty()
		{
			return new List<User>()
			{
				new User{Id=60142, Name="usuario", Surname="fichero", IdLevel=1,   IdSpecialty=3, IdUnity =10024, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60143, Name="usuario2", Surname="fichero", IdLevel=2,  IdSpecialty=3, IdUnity =10025, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60144, Name="usuario3", Surname="fichero", IdLevel=3,  IdSpecialty=3, IdUnity =10026, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60145, Name="usuario4", Surname="fichero", IdLevel=6,  IdSpecialty=3, IdUnity =10027, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60146, Name="usuario5", Surname="fichero", IdLevel=7,  IdSpecialty=3, IdUnity =1, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60147, Name="usuario6", Surname="fichero", IdLevel=1,  IdSpecialty=3, IdUnity =10024, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60148, Name="usuario7", Surname="fichero", IdLevel=1,  IdSpecialty=3, IdUnity =1, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60149, Name="usuario8", Surname="fichero", IdLevel=2,  IdSpecialty=3, IdUnity =10024, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60150, Name="usuario9", Surname="fichero", IdLevel=3,  IdSpecialty=3, IdUnity =10025, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60151, Name="usuario10", Surname="fichero", IdLevel=6, IdSpecialty=3, IdUnity =10026, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60152, Name="usuario11", Surname="fichero", IdLevel=7, IdSpecialty=3, IdUnity =10027, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60153, Name="usuario12", Surname="fichero", IdLevel=2, IdSpecialty=3, IdUnity =10025, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60154, Name="usuario13", Surname="fichero", IdLevel=1, IdSpecialty=3, IdUnity =10027, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60155, Name="usuario14", Surname="fichero", IdLevel=2, IdSpecialty=3, IdUnity =1, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60156, Name="usuario15", Surname="fichero", IdLevel=3, IdSpecialty=3, IdUnity =10024, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60157, Name="usuario16", Surname="fichero", IdLevel=6, IdSpecialty=3, IdUnity =10025, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60158, Name="usuario17", Surname="fichero", IdLevel=7, IdSpecialty=3, IdUnity =10026, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60159, Name="usuario18", Surname="fichero", IdLevel=3, IdSpecialty=3, IdUnity =10026, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60160, Name="usuario19", Surname="fichero", IdLevel=1, IdSpecialty=3, IdUnity =10026, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60161, Name="usuario20", Surname="fichero", IdLevel=2, IdSpecialty=3, IdUnity =10027, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60162, Name="usuario21", Surname="fichero", IdLevel=3, IdSpecialty=3, IdUnity =1, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60163, Name="usuario22", Surname="fichero", IdLevel=6, IdSpecialty=3, IdUnity =10024, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60164, Name="usuario23", Surname="fichero", IdLevel=7, IdSpecialty=3, IdUnity =10025, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60165, Name="usuario24", Surname="fichero", IdLevel=6, IdSpecialty=3, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60166, Name="usuario25", Surname="fichero", IdLevel=1, IdSpecialty=3, IdUnity =10025, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60167, Name="usuario26", Surname="fichero", IdLevel=2, IdSpecialty=3, IdUnity =10026, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60168, Name="usuario27", Surname="fichero", IdLevel=3, IdSpecialty=3, IdUnity =10027, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60169, Name="usuario28", Surname="fichero", IdLevel=6, IdSpecialty=3, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60170, Name="usuario29", Surname="fichero", IdLevel=7, IdSpecialty=3, IdUnity =10024, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
				new User{Id=60171, Name="usuario30", Surname="fichero", IdLevel=7, IdSpecialty=3, IdUnity =10024, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>()},
			};
		}


		private static List<User> GetFakeUserWithHolidaysTriplets()
		{
			return new List<User>()
			{
				new User{Id=60142, Name="usuario", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"}, AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110206, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		  new AskedHoliday() { Id = 110233, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		  new AskedHoliday() { Id = 110234, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60142}}
																																																		},
				new User{Id=60143, Name="usuario2", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110207, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110125, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110126, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60143}}
																																																		},
				new User{Id=60144, Name="usuario3", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110208, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110235, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110236, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60144}}
																																																		},
				new User{Id=60145, Name="usuario4", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110209, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110127, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110128, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60145}}
																																																		},
				new User{Id=60146, Name="usuario5", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110210, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110237, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110238, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60146}}
																																																	},
				new User{Id=60147, Name="usuario6", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110211, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110129, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110185, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60147}}
																																																		},
				new User{Id=60148, Name="usuario7", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110212, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110074, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110104, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60148}}
																																																	},
				new User{Id=60149, Name="usuario8", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110213, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110186, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110187, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60149}}
																																																		},
				new User{Id=60150, Name="usuario9", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110214, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110105, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110106, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60150}}
																																																		},
				new User{Id=60151, Name="usuario10", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110215, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110188, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110189, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60151}}
																																																		},
				new User{Id=60152, Name="usuario11", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110216, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110107, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110108, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60152}}
																																																		 },
				new User{Id=60153, Name="usuario12", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110217, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110190, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110191, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60153}}
																																																		 },
				new User{Id=60154, Name="usuario13", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110218, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110109, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110110, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60154}}
																																																		 },
				new User{Id=60155, Name="usuario14", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110219, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110192, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110193, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60155}}
																																																	 },
				new User{Id=60156, Name="usuario15", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110220, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110111, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110112, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60156}}
																																																		  },
				new User{Id=60157, Name="usuario16", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110221, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110194, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110195, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60157}}
																																																		 },
				new User{Id=60158, Name="usuario17", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110222, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110113, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110118, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60158}}
																																																		 },
				new User{Id=60159, Name="usuario18", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110205, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110223, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110224, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60159}}
																																																		 },
				new User{Id=60160, Name="usuario19", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110197, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110115, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110117, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60160}}
																																																		 },
				new User{Id=60161, Name="usuario20", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110199, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110226, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110228, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60161}}
																																																		 },
				new User{Id=60162, Name="usuario21", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110201, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110119, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110120, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60162} }
																																																	 },
				new User{Id=60163, Name="usuario22", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110202, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110229, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110230, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60163}}
																																																		 },
				new User{Id=60164, Name="usuario23", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110203, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110121, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110122, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60164}}
																																																		 },
				new User{Id=60165, Name="usuario24", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110204, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110231, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110239, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60165}}
																																																	 },
				new User{Id=60166, Name="usuario25", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110239, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60166}}},
				new User{Id=60167, Name="usuario26", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110240, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60167}}},
				new User{Id=60168, Name="usuario27", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110241, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60168}}},
				new User{Id=60169, Name="usuario28", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1,	  IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110242, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60169}}},
				new User{Id=60170, Name="usuario29", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110243, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60170}}},
				new User{Id=60171, Name="usuario30", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110244, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60171}}},
			};
		}

		private static List<User> GetFakeUserWithHolidaysCuatruplets()
		{
			return new List<User>()
			{
				new User{Id=60142, Name="usuario", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110306, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110307, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110308, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110309, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110310, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110311, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110312, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110313, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110314, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110315, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110316, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110317, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110318, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110319, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110320, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110321, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110322, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110323, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110324, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110325, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110326, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110327, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110328, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110329, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60142}}
				},
				new User{Id=60143, Name="usuario2", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110330, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110331, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110332, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110333, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110334, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110335, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110336, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110337, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110338, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110339, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110340, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110341, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110342, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110343, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110344, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110345, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110346, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110347, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110348, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110349, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110350, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110351, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110352, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110353, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60143}}
				},
				new User{Id=60144, Name="usuario3", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110354, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110355, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110356, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110357, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110358, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110359, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110360, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110361, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110362, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110363, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110364, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110365, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110366, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110367, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110368, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110369, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110370, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110371, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110372, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110373, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110374, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110375, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110376, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110377, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60144}}
				},
				new User{Id=60145, Name="usuario4", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110378, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110379, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110380, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110381, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110382, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110383, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110384, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110385, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110386, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110387, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110388, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110389, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110390, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110391, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110392, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110393, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110394, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110395, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110396, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110397, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110398, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110399, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110400, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110401, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60145}}
				},
				new User{Id=60146, Name="usuario5", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110402, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110403, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110404, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110405, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110406, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110407, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110408, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110409, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110410, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110411, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110412, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110413, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110414, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110415, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110416, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110417, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110418, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110419, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110420, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110421, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110422, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110423, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110424, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110425, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60146}}
				},
				new User{Id=60147, Name="usuario6", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110426, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110427, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110428, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110429, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110430, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110431, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110432, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110433, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110434, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110435, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110436, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110437, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110438, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110439, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110440, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110441, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110442, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110443, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110444, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110445, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110446, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110447, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110448, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110449, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60147}}
				},
				new User{Id=60148, Name="usuario7", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110450, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110451, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110452, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110453, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110454, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110455, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110456, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110457, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110458, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110459, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110460, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110461, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110462, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110463, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110464, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110465, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110466, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110467, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110468, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110469, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110470, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110471, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110472, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110473, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60148}}
				},
				new User{Id=60149, Name="usuario8", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110474, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110475, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110476, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110477, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110478, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110479, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110480, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110481, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110482, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110483, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110484, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110485, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110486, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110487, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110488, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110489, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110490, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110491, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110492, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110493, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110494, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110495, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110496, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110497, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60149}}
				},
				new User{Id=60150, Name="usuario9", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110498, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110499, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110500, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110501, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110502, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110503, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110504, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110505, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110506, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110507, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110508, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110509, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110510, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110511, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110512, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110513, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110514, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110515, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110516, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110517, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110518, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110519, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110520, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110521, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60150}}
				},
				new User{Id=60151, Name="usuario10", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110522, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110523, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110524, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110525, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110526, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110527, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110528, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110529, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110530, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110531, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110532, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110533, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110534, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110535, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110536, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110537, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110538, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110539, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110540, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110541, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110542, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110543, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110544, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110545, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60151}}
				},
				new User{Id=60152, Name="usuario11", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110546, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110547, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110548, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110549, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110550, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110551, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110552, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110553, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110554, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110555, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110556, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110557, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110558, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110559, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110560, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110561, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110562, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110563, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110564, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110565, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110566, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110567, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110568, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110569, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60152}}
				},
				new User{Id=60153, Name="usuario12", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110570, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110571, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110572, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110573, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110574, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110575, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110576, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110577, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110578, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110579, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110580, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110581, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110582, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110583, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110584, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110585, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110586, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110587, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110588, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110589, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110590, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110591, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110592, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110593, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60153}}
				},
				new User{Id=60154, Name="usuario13", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110594, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110595, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110596, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110597, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110598, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110599, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110600, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110601, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110602, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110603, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110604, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110605, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110606, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110607, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110608, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110609, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110610, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110611, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110612, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110613, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110614, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110615, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110616, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110617, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60154}}
				},
				new User{Id=60155, Name="usuario14", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110618, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110619, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110620, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110621, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110622, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110623, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110624, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110625, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110626, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110627, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110628, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110629, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110630, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110631, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110632, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110633, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110634, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110635, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110636, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110637, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110638, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110639, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110640, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110641, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60155}}
				},
				new User{Id=60156, Name="usuario15", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110642, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110643, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110644, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110645, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110646, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110647, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110648, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110649, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110650, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110651, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110652, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110653, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110654, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110655, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110656, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110657, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110658, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110659, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110660, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110661, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110662, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110663, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110664, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110665, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60156}}
				},
				new User{Id=60157, Name="usuario16", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110666, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110667, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110668, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110669, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110670, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110671, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110672, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110673, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110674, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110675, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110676, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110677, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110678, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110679, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110680, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110681, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110682, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110683, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110684, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110685, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110686, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110687, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110688, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110689, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60157}}
				},
				new User{Id=60158, Name="usuario17", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110690, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110691, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110692, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110693, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110694, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110695, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110696, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110697, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110698, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110699, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110700, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110701, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110702, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110703, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110704, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110705, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110706, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110707, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110708, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110709, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110710, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110711, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110712, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110713, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60158}}
				},
				new User{Id=60159, Name="usuario18", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110714, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110715, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110716, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110717, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110718, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110719, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110720, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110721, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110722, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110723, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110724, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110725, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110726, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110727, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110728, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110729, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110730, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110731, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110732, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110733, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110734, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110735, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110736, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110737, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60159}}
				},
				new User{Id=60160, Name="usuario19", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110738, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110739, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110740, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110741, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110742, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110743, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110744, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110745, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110746, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110747, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110748, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110749, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110750, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110751, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110752, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110753, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110754, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110755, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110756, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110757, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110758, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110759, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110760, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110761, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60160}}
				},
				new User{Id=60161, Name="usuario20", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110762, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110763, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110764, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110765, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110766, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110767, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110768, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110769, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110770, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110771, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110772, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110773, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110774, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110775, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110776, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110777, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110778, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110779, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110780, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110781, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110782, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110783, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110784, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110785, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60161}}
				},
				new User{Id=60162, Name="usuario21", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110786, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110787, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110788, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110789, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110790, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110791, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110792, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110793, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110794, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110795, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110796, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110797, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110798, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110799, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110800, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110801, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110802, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110803, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110804, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110805, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110806, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110807, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110808, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110809, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60162}}
				},
				new User{Id=60163, Name="usuario22", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110810, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110811, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110812, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110813, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110814, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110815, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110816, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110817, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110818, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110819, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110820, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110821, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110822, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110823, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110824, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110825, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110826, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110827, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110828, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110829, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110830, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110831, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110832, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110833, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60163}}
				},
				new User{Id=60164, Name="usuario23", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110834, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110835, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110836, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110837, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110838, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110839, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110840, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110841, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110842, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110843, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110844, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110845, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110846, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110847, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110848, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110849, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110850, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110851, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110852, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110853, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110854, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110855, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110856, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110857, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60164}}
				},
				new User{Id=60165, Name="usuario24", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110858, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110859, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110860, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110861, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110862, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110863, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110864, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110865, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110866, DateFrom = new DateOnly(2024,1,11), DateTo = new DateOnly(2024,1,11), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110867, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110868, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110869, DateFrom = new DateOnly(2024,1,15), DateTo = new DateOnly(2024,1,15), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110870, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110871, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110872, DateFrom = new DateOnly(2024,1,19), DateTo = new DateOnly(2024,1,19), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110873, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110874, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110875, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110876, DateFrom = new DateOnly(2024,1,24), DateTo = new DateOnly(2024,1,24), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110877, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110878, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110879, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110880, DateFrom = new DateOnly(2024,1,28), DateTo = new DateOnly(2024,1,28), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110881, DateFrom = new DateOnly(2024,1,30), DateTo = new DateOnly(2024,1,30), IdStatus = 3, Period = "2024", IdUser = 60165}}
				},
				new User{Id=60166, Name="usuario25", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110882, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110883, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110884, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110885, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110886, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110887, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110888, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110889, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110890, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110891, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110892, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110893, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110894, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110895, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110896, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110897, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110898, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110899, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110900, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110901, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110902, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110903, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110904, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110905, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60166}}
				},
				new User{Id=60167, Name="usuario26", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110906, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110907, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110908, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110909, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110910, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110911, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110912, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110913, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110914, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110915, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110916, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110917, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110918, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110919, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110920, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110921, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110922, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110923, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110924, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110925, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110926, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110927, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110928, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110929, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60167}}
				},
				new User{Id=60168, Name="usuario27", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110930, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110931, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110932, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110933, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110934, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110935, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110936, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110937, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110938, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110939, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110940, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110941, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110942, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110943, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110944, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110945, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110946, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110947, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110948, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110949, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110950, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110951, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110952, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110953, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60168}}
				},
				new User{Id=60169, Name="usuario28", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110954, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110955, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110956, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110957, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110958, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110959, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110960, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110961, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110962, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110963, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110964, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110965, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110966, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110967, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110968, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110969, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110970, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110971, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110972, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110973, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110974, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110975, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110976, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110977, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60169}}
				},
				new User{Id=60170, Name="usuario29", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110978, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110979, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110980, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110981, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110982, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110983, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110984, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110985, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110986, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110987, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110988, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110989, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110990, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110991, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110992, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110993, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110994, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110995, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110996, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110997, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110998, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110999, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 111000, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 111001, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60170}}
				},
				new User{Id=60171, Name="usuario30", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 111002, DateFrom = new DateOnly(2024,1,1), DateTo = new DateOnly(2024,1,1), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111003, DateFrom = new DateOnly(2024,1,2), DateTo = new DateOnly(2024,1,2), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111004, DateFrom = new DateOnly(2024,1,3), DateTo = new DateOnly(2024,1,3), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111005, DateFrom = new DateOnly(2024,1,4), DateTo = new DateOnly(2024,1,4), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111006, DateFrom = new DateOnly(2024,1,5), DateTo = new DateOnly(2024,1,5), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111007, DateFrom = new DateOnly(2024,1,6), DateTo = new DateOnly(2024,1,6), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111008, DateFrom = new DateOnly(2024,1,7), DateTo = new DateOnly(2024,1,7), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111009, DateFrom = new DateOnly(2024,1,8), DateTo = new DateOnly(2024,1,8), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111010, DateFrom = new DateOnly(2024,1,9), DateTo = new DateOnly(2024,1,9), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111011, DateFrom = new DateOnly(2024,1,10), DateTo = new DateOnly(2024,1,10), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111012, DateFrom = new DateOnly(2024,1,12), DateTo = new DateOnly(2024,1,12), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111013, DateFrom = new DateOnly(2024,1,13), DateTo = new DateOnly(2024,1,13), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111014, DateFrom = new DateOnly(2024,1,14), DateTo = new DateOnly(2024,1,14), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111015, DateFrom = new DateOnly(2024,1,16), DateTo = new DateOnly(2024,1,16), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111016, DateFrom = new DateOnly(2024,1,17), DateTo = new DateOnly(2024,1,17), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111017, DateFrom = new DateOnly(2024,1,18), DateTo = new DateOnly(2024,1,18), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111018, DateFrom = new DateOnly(2024,1,20), DateTo = new DateOnly(2024,1,20), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111019, DateFrom = new DateOnly(2024,1,21), DateTo = new DateOnly(2024,1,21), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111020, DateFrom = new DateOnly(2024,1,22), DateTo = new DateOnly(2024,1,22), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111021, DateFrom = new DateOnly(2024,1,23), DateTo = new DateOnly(2024,1,23), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111022, DateFrom = new DateOnly(2024,1,25), DateTo = new DateOnly(2024,1,25), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111023, DateFrom = new DateOnly(2024,1,26), DateTo = new DateOnly(2024,1,26), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111024, DateFrom = new DateOnly(2024,1,27), DateTo = new DateOnly(2024,1,27), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111025, DateFrom = new DateOnly(2024,1,29), DateTo = new DateOnly(2024,1,29), IdStatus = 3, Period = "2024", IdUser = 60171}}
				},
			};
		}

		private static List<User> GetFakeUserWithHolidaysCuatrupletsCurrentMonth()
		{
			return new List<User>()
			{
				new User{Id=60142, Name="usuario", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110306, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110307, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110308, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110309, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110310, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110311, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110312, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110313, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110314, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110315, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110316, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110317, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110318, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110319, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110320, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110321, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110322, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110323, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110324, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110325, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110326, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110327, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110328, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60142},
																																																		 new AskedHoliday() { Id = 110329, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60142}}
				},
				new User{Id=60143, Name="usuario2", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110330, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110331, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110332, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110333, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110334, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110335, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110336, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110337, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110338, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110339, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110340, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110341, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110342, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110343, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110344, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110345, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110346, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110347, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110348, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110349, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110350, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110351, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110352, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60143},
																																																		  new AskedHoliday() { Id = 110353, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60143}}
				},
				new User{Id=60144, Name="usuario3", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110354, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110355, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110356, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110357, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110358, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110359, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110360, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110361, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110362, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110363, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110364, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110365, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110366, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110367, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110368, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110369, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110370, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110371, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110372, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110373, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110374, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110375, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110376, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60144},
																																																		  new AskedHoliday() { Id = 110377, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60144}}
				},
				new User{Id=60145, Name="usuario4", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110378, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110379, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110380, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110381, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110382, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110383, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110384, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110385, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110386, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110387, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110388, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110389, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110390, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110391, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110392, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110393, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110394, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110395, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110396, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110397, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110398, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110399, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110400, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60145},
																																																		  new AskedHoliday() { Id = 110401, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60145}}
				},
				new User{Id=60146, Name="usuario5", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110402, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110403, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110404, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110405, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110406, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110407, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110408, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110409, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110410, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110411, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110412, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110413, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110414, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110415, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110416, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110417, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110418, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110419, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110420, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110421, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110422, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110423, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110424, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60146},
																																																	  new AskedHoliday() { Id = 110425, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60146}}
				},
				new User{Id=60147, Name="usuario6", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110426, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110427, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110428, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110429, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110430, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110431, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110432, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110433, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110434, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110435, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110436, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110437, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110438, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110439, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110440, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110441, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110442, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110443, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110444, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110445, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110446, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110447, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110448, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60147},
																																																		  new AskedHoliday() { Id = 110449, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60147}}
				},
				new User{Id=60148, Name="usuario7", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110450, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110451, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110452, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110453, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110454, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110455, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110456, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110457, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110458, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110459, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110460, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110461, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110462, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110463, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110464, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110465, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110466, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110467, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110468, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110469, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110470, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110471, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110472, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60148},
																																																	  new AskedHoliday() { Id = 110473, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60148}}
				},
				new User{Id=60149, Name="usuario8", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110474, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110475, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110476, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110477, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110478, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110479, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110480, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110481, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110482, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110483, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110484, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110485, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110486, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110487, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110488, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110489, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110490, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110491, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110492, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110493, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110494, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110495, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110496, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60149},
																																																		  new AskedHoliday() { Id = 110497, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60149}}
				},
				new User{Id=60150, Name="usuario9", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110498, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110499, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110500, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110501, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110502, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110503, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110504, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110505, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110506, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110507, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110508, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110509, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110510, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110511, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110512, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110513, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110514, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110515, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110516, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110517, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110518, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110519, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110520, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60150},
																																																		  new AskedHoliday() { Id = 110521, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60150}}
				},
				new User{Id=60151, Name="usuario10", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110522, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110523, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110524, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110525, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110526, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110527, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110528, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110529, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110530, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110531, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110532, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110533, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110534, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110535, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110536, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110537, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110538, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110539, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110540, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110541, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110542, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110543, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110544, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60151},
																																																		   new AskedHoliday() { Id = 110545, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60151}}
				},
				new User{Id=60152, Name="usuario11", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110546, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110547, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110548, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110549, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110550, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110551, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110552, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110553, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110554, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110555, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110556, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110557, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110558, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110559, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110560, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110561, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110562, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110563, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110564, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110565, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110566, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110567, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110568, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60152},
																																																		   new AskedHoliday() { Id = 110569, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60152}}
				},
				new User{Id=60153, Name="usuario12", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110570, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110571, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110572, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110573, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110574, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110575, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110576, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110577, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110578, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110579, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110580, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110581, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110582, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110583, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110584, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110585, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110586, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110587, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110588, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110589, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110590, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110591, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110592, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60153},
																																																		   new AskedHoliday() { Id = 110593, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60153}}
				},
				new User{Id=60154, Name="usuario13", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110594, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110595, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110596, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110597, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110598, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110599, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110600, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110601, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110602, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110603, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110604, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110605, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110606, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110607, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110608, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110609, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110610, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110611, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110612, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110613, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110614, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110615, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110616, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60154},
																																																		   new AskedHoliday() { Id = 110617, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60154}}
				},
				new User{Id=60155, Name="usuario14", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110618, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110619, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110620, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110621, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110622, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110623, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110624, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110625, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110626, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110627, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110628, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110629, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110630, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110631, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110632, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110633, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110634, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110635, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110636, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110637, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110638, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110639, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110640, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60155},
																																																	   new AskedHoliday() { Id = 110641, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60155}}
				},
				new User{Id=60156, Name="usuario15", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110642, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110643, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110644, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110645, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110646, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110647, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110648, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110649, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110650, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110651, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110652, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110653, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110654, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110655, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110656, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110657, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110658, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110659, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110660, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110661, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110662, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110663, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110664, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60156},
																																																		   new AskedHoliday() { Id = 110665, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60156}}
				},
				new User{Id=60157, Name="usuario16", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110666, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110667, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110668, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110669, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110670, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110671, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110672, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110673, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110674, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110675, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110676, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110677, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110678, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110679, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110680, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110681, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110682, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110683, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110684, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110685, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110686, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110687, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110688, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60157},
																																																		   new AskedHoliday() { Id = 110689, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60157}}
				},
				new User{Id=60158, Name="usuario17", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110690, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110691, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110692, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110693, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110694, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110695, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110696, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110697, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110698, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110699, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110700, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110701, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110702, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110703, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110704, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110705, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110706, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110707, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110708, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110709, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110710, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110711, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110712, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60158},
																																																		   new AskedHoliday() { Id = 110713, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60158}}
				},
				new User{Id=60159, Name="usuario18", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110714, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110715, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110716, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110717, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110718, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110719, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110720, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110721, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110722, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110723, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110724, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110725, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110726, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110727, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110728, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110729, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110730, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110731, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110732, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110733, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110734, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110735, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110736, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60159},
																																																		   new AskedHoliday() { Id = 110737, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60159}}
				},
				new User{Id=60160, Name="usuario19", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110738, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110739, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110740, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110741, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110742, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110743, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110744, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110745, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110746, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110747, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110748, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110749, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110750, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110751, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110752, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110753, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110754, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110755, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110756, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110757, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110758, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110759, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110760, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60160},
																																																		   new AskedHoliday() { Id = 110761, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60160}}
				},
				new User{Id=60161, Name="usuario20", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110762, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110763, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110764, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110765, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110766, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110767, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110768, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110769, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110770, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110771, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110772, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110773, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110774, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110775, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110776, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110777, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110778, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110779, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110780, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110781, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110782, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110783, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110784, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60161},
																																																		   new AskedHoliday() { Id = 110785, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60161}}
				},
				new User{Id=60162, Name="usuario21", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110786, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110787, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110788, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110789, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110790, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110791, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110792, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110793, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110794, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110795, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110796, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110797, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110798, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110799, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110800, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110801, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110802, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110803, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110804, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110805, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110806, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110807, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110808, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60162},
																																																	   new AskedHoliday() { Id = 110809, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60162}}
				},
				new User{Id=60163, Name="usuario22", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110810, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110811, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110812, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110813, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110814, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110815, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110816, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110817, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110818, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110819, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110820, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110821, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110822, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110823, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110824, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110825, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110826, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110827, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110828, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110829, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110830, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110831, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110832, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60163},
																																																		   new AskedHoliday() { Id = 110833, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60163}}
				},
				new User{Id=60164, Name="usuario23", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110834, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110835, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110836, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110837, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110838, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110839, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110840, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110841, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110842, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110843, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110844, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110845, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110846, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110847, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110848, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110849, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110850, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110851, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110852, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110853, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110854, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110855, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110856, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60164},
																																																		   new AskedHoliday() { Id = 110857, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60164}}
				},
				new User{Id=60165, Name="usuario24", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110858, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110859, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110860, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110861, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110862, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110863, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110864, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110865, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110866, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,11), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,11), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110867, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110868, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110869, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,15), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,15), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110870, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110871, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110872, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,19), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,19), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110873, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110874, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110875, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110876, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,24), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,24), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110877, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110878, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110879, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110880, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,28), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,28), IdStatus = 3, Period = "2024", IdUser = 60165},
																																																	   new AskedHoliday() { Id = 110881, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,30), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,30), IdStatus = 3, Period = "2024", IdUser = 60165}}
				},
				new User{Id=60166, Name="usuario25", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110882, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110883, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110884, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110885, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110886, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110887, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110888, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110889, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110890, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110891, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110892, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110893, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110894, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110895, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110896, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110897, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110898, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110899, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110900, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110901, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110902, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110903, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110904, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60166},
																																																		   new AskedHoliday() { Id = 110905, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60166}}
				},
				new User{Id=60167, Name="usuario26", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110906, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110907, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110908, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110909, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110910, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110911, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110912, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110913, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110914, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110915, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110916, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110917, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110918, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110919, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110920, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110921, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110922, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110923, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110924, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110925, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110926, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110927, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110928, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60167},
																																																		   new AskedHoliday() { Id = 110929, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60167}}
				},
				new User{Id=60168, Name="usuario27", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110930, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110931, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110932, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110933, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110934, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110935, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110936, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110937, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110938, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110939, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110940, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110941, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110942, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110943, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110944, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110945, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110946, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110947, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110948, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110949, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110950, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110951, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110952, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60168},
																																																		   new AskedHoliday() { Id = 110953, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60168}}
				},
				new User{Id=60169, Name="usuario28", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110954, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110955, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110956, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110957, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110958, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110959, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110960, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110961, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110962, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110963, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110964, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110965, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110966, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110967, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110968, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110969, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110970, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110971, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110972, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110973, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110974, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110975, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110976, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60169},
																																																	   new AskedHoliday() { Id = 110977, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60169}}
				},
				new User{Id=60170, Name="usuario29", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110978, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110979, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110980, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110981, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110982, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110983, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110984, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110985, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110986, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110987, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110988, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110989, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110990, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110991, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110992, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110993, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110994, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110995, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110996, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110997, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110998, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 110999, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 111000, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60170},
																																																		   new AskedHoliday() { Id = 111001, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60170}}
				},
				new User{Id=60171, Name="usuario30", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 111002, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,1), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,1), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111003, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,2), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,2), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111004, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,3), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,3), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111005, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,4), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,4), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111006, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,5), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,5), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111007, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,6), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,6), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111008, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,7), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,7), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111009, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,8), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,8), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111010, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,9), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,9), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111011, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,10), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,10), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111012, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,12), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,12), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111013, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,13), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,13), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111014, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,14), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,14), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111015, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,16), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,16), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111016, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,17), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,17), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111017, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,18), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,18), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111018, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,20), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,20), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111019, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,21), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,21), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111020, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,22), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,22), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111021, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,23), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,23), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111022, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,25), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,25), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111023, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,26), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,26), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111024, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,27), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,27), IdStatus = 3, Period = "2024", IdUser = 60171},
																																																		   new AskedHoliday() { Id = 111025, DateFrom = new DateOnly(2024,DateTime.Now.Month + 1,29), DateTo = new DateOnly(2024,DateTime.Now.Month + 1,29), IdStatus = 3, Period = "2024", IdUser = 60171}}
				},
			};
		}

		private static List<User> GetFakeUserWithHolidaysNotSolution()
		{
			return new List<User>()
			{
				new User{Id=60142, Name="usuario", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004,   IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110206, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60142}}},
				new User{Id=60143, Name="usuario2", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005,  IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110207, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60143}}},
				new User{Id=60144, Name="usuario3", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006,  IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110208, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60144}}},
				new User{Id=60145, Name="usuario4", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10007,  IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110209, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60145}}},
				new User{Id=60146, Name="usuario5", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =1,	  IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110210, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60146}}},
				new User{Id=60147, Name="usuario6", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10004,  IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110211, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60147}}},
				new User{Id=60148, Name="usuario7", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =1,	  IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110212, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60148}}},
				new User{Id=60149, Name="usuario8", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10004,  IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110213, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60149}}},
				new User{Id=60150, Name="usuario9", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10005,  IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110214, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60150}}},
				new User{Id=60151, Name="usuario10", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110215, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60151}}},
				new User{Id=60152, Name="usuario11", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110216, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60152}}},
				new User{Id=60153, Name="usuario12", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110217, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60153}}},
				new User{Id=60154, Name="usuario13", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110218, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60154}}},
				new User{Id=60155, Name="usuario14", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =1,	  IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110219, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60155}}},
				new User{Id=60156, Name="usuario15", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110220, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60156}}},
				new User{Id=60157, Name="usuario16", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110221, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60157}}},
				new User{Id=60158, Name="usuario17", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110222, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60158}}},
				new User{Id=60159, Name="usuario18", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110223, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60159}}},
				new User{Id=60160, Name="usuario19", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110224, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60160}}},
				new User{Id=60161, Name="usuario20", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110225, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60161}}},
				new User{Id=60162, Name="usuario21", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =1,	  IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110226, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60162}}},
				new User{Id=60163, Name="usuario22", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110227, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60163}}},
				new User{Id=60164, Name="usuario23", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110228, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60164}}},
				new User{Id=60165, Name="usuario24", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1,	  IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110229, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60165}}},
				new User{Id=60166, Name="usuario25", Surname="fichero", IdLevel=1, IdSpecialty=1, IdUnity =10005, IdLevelNavigation=new Level(){Id=1, Name="R1"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110230, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60166}}},
				new User{Id=60167, Name="usuario26", Surname="fichero", IdLevel=2, IdSpecialty=1, IdUnity =10006, IdLevelNavigation=new Level(){Id=2, Name="R2"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110231, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60167}}},
				new User{Id=60168, Name="usuario27", Surname="fichero", IdLevel=3, IdSpecialty=1, IdUnity =10007, IdLevelNavigation=new Level(){Id=3, Name="R3"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110232, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60168}}},
				new User{Id=60169, Name="usuario28", Surname="fichero", IdLevel=6, IdSpecialty=1, IdUnity =1,     IdLevelNavigation=new Level(){Id=6, Name="R4"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110233, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60169}}},
				new User{Id=60170, Name="usuario29", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110234, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60170}}},
				new User{Id=60171, Name="usuario30", Surname="fichero", IdLevel=7, IdSpecialty=1, IdUnity =10004, IdLevelNavigation=new Level(){Id=7, Name="R5"},AskedHolidays = new List<AskedHoliday>(){ new AskedHoliday() { Id = 110235, DateFrom = new DateOnly(2024,9,23), DateTo = new DateOnly(2024,9,23), IdStatus = 3, Period = "2024", IdUser = 60171}}},
			};
		}

		private List<Specialty> GetFakeSpecialties()
		{
			return new List<Specialty>()
			{
				new Specialty
				{
					Id = 1,
					IdCenter = 1,
					MaxGuards = 6,
					Unities = new List<Unity>()
								{
									new Unity()
									{
										Id = 10004,
										Name = "Colon",
										IdSpecialty = 1,
										MaxByDay = 2,
										MaxByDayWeekend = 3,
									},
									new Unity()
									{
										Id = 10005,
										Name = "Hepáticobilio",
										IdSpecialty = 1,
										MaxByDay = 2,
										MaxByDayWeekend = 3,
									},
									new Unity()
									{
										Id = 10007,
										Name = "Esófago gástrica",
										IdSpecialty = 1,
										MaxByDay = 1,
										MaxByDayWeekend = 3,
									},
									new Unity()
									{
										Id = 10006,
										Name = "Endocrino",
										IdSpecialty = 1,
										MaxByDay = 2,
										MaxByDayWeekend = 3,
									}
								}
				},
				new Specialty
				{
					Id = 2,
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 10017,
							Name = "Esófago gástrica",
							IdSpecialty = 2,
							MaxByDay = 1,
							MaxByDayWeekend = 3,
						},
						new Unity()
						{
							Id = 10016,
							Name = "Endocrino",
							IdSpecialty = 2,
							MaxByDay = 2,
							MaxByDayWeekend = 3,
						}
					}
				},
				new Specialty
				{
					Id = 3,
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 10024,
							Name = "Colon",
							IdSpecialty = 3,
							MaxByDay = 2,
							MaxByDayWeekend = 3,
						},
						new Unity()
						{
							Id = 10025,
							Name = "Hepáticobilio",
							IdSpecialty = 3,
							MaxByDay = 2,
							MaxByDayWeekend = 3,
						},
						new Unity()
						{
							Id = 10027,
							Name = "Esófago gástrica",
							IdSpecialty = 3,
							MaxByDay = 1,
							MaxByDayWeekend = 3,
						},
						new Unity()
						{
							Id = 10026,
							Name = "Endocrino",
							IdSpecialty = 3,
							MaxByDay = 2,
							MaxByDayWeekend = 3,
						}
					}
				}
			};
		}

		private List<Unity> GetFakeCommonUnities()
		{
			return new List<Unity>()
					{
						new Unity()
						{
							Id = 1,
							Name = "Rotatorio",
							MaxByDay = 2,
							MaxByDayWeekend = 3,
						}
					};
		}

		private List<DayGuard> GetFakeDayGuards()
		{
			return new List<DayGuard>()
			{
				new DayGuard{Id=1, Day = new DateOnly(2024, 1,1), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=2, Day = new DateOnly(2024, 1,2), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=3, Day = new DateOnly(2024, 1,3), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=19, Day = new DateOnly(2024, 1,4), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=25, Day = new DateOnly(2024, 1,5), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=31, Day = new DateOnly(2024, 1,6), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=37, Day = new DateOnly(2024, 1,7), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=43, Day = new DateOnly(2024, 1,8), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=49, Day = new DateOnly(2024, 1,9), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=55, Day = new DateOnly(2024, 1,10), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=61, Day = new DateOnly(2024, 1,11), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=67, Day = new DateOnly(2024, 1,12), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=73, Day = new DateOnly(2024, 1,13), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=79, Day = new DateOnly(2024, 1,14), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=85, Day = new DateOnly(2024, 1,15), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=91, Day = new DateOnly(2024, 1,16), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=97, Day = new DateOnly(2024, 1,17), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=103, Day = new DateOnly(2024, 1,18), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=109, Day = new DateOnly(2024, 1,19), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=115, Day = new DateOnly(2024, 1,20), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=121, Day = new DateOnly(2024, 1,21), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=127, Day = new DateOnly(2024, 1,22), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=133, Day = new DateOnly(2024, 1,23), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=139, Day = new DateOnly(2024, 1,24), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=145, Day = new DateOnly(2024, 1,25), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=151, Day = new DateOnly(2024, 1,26), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=157, Day = new DateOnly(2024, 1,27), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=163, Day = new DateOnly(2024, 1,28), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=169, Day = new DateOnly(2024, 1,29), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=175, Day = new DateOnly(2024, 1,30), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=181, Day = new DateOnly(2024, 1,31), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=361, Day = new DateOnly(2024, 5,1), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},				
				new DayGuard{Id=361, Day = new DateOnly(2024, 10,25), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},

}},
				new DayGuard{Id=1, Day = new DateOnly(2024, 8,1), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=2, Day = new DateOnly(2024, 8,2), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=3, Day = new DateOnly(2024, 8,3), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=19, Day = new DateOnly(2024, 8,4), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=25, Day = new DateOnly(2024, 8,5), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=31, Day = new DateOnly(2024, 8,6), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=37, Day = new DateOnly(2024, 8,7), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=43, Day = new DateOnly(2024, 8,8), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=49, Day = new DateOnly(2024, 8,9), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=55, Day = new DateOnly(2024, 8,10), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=61, Day = new DateOnly(2024, 8,11), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=67, Day = new DateOnly(2024, 8,12), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=73, Day = new DateOnly(2024, 8,13), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=79, Day = new DateOnly(2024, 8,14), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=85, Day = new DateOnly(2024, 8,15), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=91, Day = new DateOnly(2024, 8,16), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=97, Day = new DateOnly(2024, 8,17), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=103, Day = new DateOnly(2024, 8,18), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=109, Day = new DateOnly(2024, 8,19), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=115, Day = new DateOnly(2024, 8,20), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=121, Day = new DateOnly(2024, 8,21), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=127, Day = new DateOnly(2024, 8,22), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=133, Day = new DateOnly(2024, 8,23), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=139, Day = new DateOnly(2024, 8,24), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=145, Day = new DateOnly(2024, 8,25), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=151, Day = new DateOnly(2024, 8,26), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=157, Day = new DateOnly(2024, 8,27), assignedUsers=new List<User>(){new User(){Id=60148, Name="usuario7", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60149, Name="usuario8", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60150, Name="usuario9", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60151, Name="usuario10", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60152, Name="usuario11", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60153, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=163, Day = new DateOnly(2024, 8,28), assignedUsers=new List<User>(){new User(){Id=60154, Name="usuario13", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60155, Name="usuario14", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60156, Name="usuario15", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60157, Name="usuario16", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60158, Name="usuario17", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60159, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=169, Day = new DateOnly(2024, 8,29), assignedUsers=new List<User>(){new User(){Id=60160, Name="usuario19", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60161, Name="usuario20", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60162, Name="usuario21", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60163, Name="usuario22", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60164, Name="usuario23", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60165, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=175, Day = new DateOnly(2024, 8,30), assignedUsers=new List<User>(){new User(){Id=60166, Name="usuario25", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60167, Name="usuario26", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60168, Name="usuario27", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60169, Name="usuario28", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60170, Name="usuario29", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60171, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
}},
				new DayGuard{Id=181, Day = new DateOnly(2024, 8,31), assignedUsers=new List<User>(){new User(){Id=60142, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
new User(){Id=60143, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
new User(){Id=60144, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
new User(){Id=60145, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
new User(){Id=60146, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
new User(){Id=60147, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
}},
			};
		}

		private static object[] GetAddGuardsCase =
		{
			new object[] 
			{ 
				new DayGuard
				{
					Day = new DateOnly(2024, 01, 01),
					assignedUsers = new List<User>()
					{
						new User()
						{
							Id = 1,
							Name = "usuario1",
							IdCenter = 1,
							IdLevel = 1,
							IdSpecialty = 1,
							Surname = "usuario"
						}
					}
				}, true
			},
			new object[] 
			{ 
				new DayGuard
				{
					Day = new DateOnly(2024, 03, 01),
					assignedUsers = new List<User>()
					{
						new User()
						{
							Id = 1,
							Name = "usuario1",
							IdCenter = 1,
							IdLevel = 1,
							IdSpecialty = 1,
							Surname = "usuario"
						}
					}
				}, true
			}
		};

		//private static object[] GetUserStatsCase =
		//{
		//	new object[] 
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 0,
		//			groupOfWeeks = 1,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 1,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 2,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 0,
		//			groupOfWeeks = 0,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 0,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 1,
		//			year = 2024
		//		}, "No se pueden asignar las guardias del grupo de semanas ", GetFakeUserWithHolidaysCuatruplets(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 2,
		//			year = 2024
		//		}, "Error al guardar la guardia", GetFakeUserWithHolidays(), false, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 2,
		//			year = 2024
		//		}, "Error al borrar la guardia previamente calculada", GetFakeUserWithHolidays(), false, false
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 0,
		//			groupOfWeeks = 5,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 0,
		//			groupOfWeeks = 11,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 0,
		//			groupOfWeeks = 1,
		//			year = 2024
		//		}, "No se pueden asignar las guardias del grupo de semanas ", GetFakeUserWithHolidaysTriplets(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 1,
		//			year = 2024
		//		}, "No se pueden asignar las guardias del grupo de semanas ", GetFakeUserWithHolidaysTriplets(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 0,
		//			groupOfWeeks = 0,
		//			year = 2024
		//		}, "No se pueden asignar las guardias del grupo de semanas ", GetFakeUserWithHolidaysNotSolution(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 0,
		//			year = 2024
		//		}, "No se pueden asignar las guardias del grupo de semanas ", GetFakeUserWithHolidaysNotSolution(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 1,
		//			groupOfWeeks = 0,
		//			year = 2025
		//		}, "OK", GetFakeUserWithHolidays(), true, true
		//	},
		//	new object[]
		//	{
		//		new GuardRequest()
		//		{
		//			idCenter = 1,
		//			idSpecialty = 0,
		//			groupOfWeeks = 1,
		//			year = 2024
		//		}, "OK", GetFakeUserWithHolidaysSpecialty(), true, true
		//	}
		//};

		private static object[] GetUserStatsCase =
		{
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 1,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 1,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 2,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 9,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 0,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 0,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 1,
					year = 2024
				}, "No se pueden asignar las guardias del mes 1", GetFakeUserWithHolidaysCuatruplets(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 0,
					year = 2024
				}, "No se pueden asignar las guardias del mes 10", GetFakeUserWithHolidaysCuatrupletsCurrentMonth(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 2,
					year = 2024
				}, "Error al guardar la guardia", GetFakeUserWithHolidays(), false, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 2,
					year = 2024
				}, "Error al borrar la guardia previamente calculada", GetFakeUserWithHolidays(), false, false
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 5,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 11,
					year = 2024
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 1,
					year = 2024
				}, "OK", GetFakeUserWithHolidaysTriplets(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 1,
					year = 2024
				}, "OK", GetFakeUserWithHolidaysTriplets(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 0,
					year = 2024
				}, "OK", GetFakeUserWithHolidaysNotSolution(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 0,
					year = 2024
				}, "OK", GetFakeUserWithHolidaysNotSolution(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 0,
					year = 2025
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 1,
					year = 2024
				}, "OK", GetFakeUserWithHolidaysSpecialty(), true, true
			}
		};

		//private static object[] GetDeleteGuardsCase =
		//{
		//	new object[] { new GuardInterval()
		//	{
		//		firstDayInterval = new DateOnly(2024, 1, 1),
		//		lastDayInterval = new DateOnly(2024, 1, 28)
		//	}, true},
		//	new object[] { new GuardInterval()
		//	{
		//		firstDayInterval = new DateOnly(2024, 12, 30),
		//		lastDayInterval =  new DateOnly(2025, 1, 26)
		//	}, true},
		//};

		private static object[] GetDeleteGuardsCase =
		{
			new object[] { 1, true},
			new object[] { 12, true},
		};

		private static object[] GetGuardsCase =
		{
			new object[] 
			{ 
				new List<DayGuard>()
				{
					new DayGuard
					{
						Id = 1,
						Day = new DateOnly(2024, 01, 01),
						assignedUsers = new List<User>()
						{
							new User()
							{
								Id = 1,
								Name = "usuario1",
								IdCenter = 1,
								IdLevel = 1,
								IdSpecialty = 1,
								Surname = "usuario"
							}
						}
					}
				},
				new List<DayGuardModel>()
				{
					new DayGuardModel
					{
						Id = 1,
						Day = new DateOnly(2024, 01, 01),
						assignedUsers = new List<UserModel>()
						{
							new UserModel()
							{
								Id = 1,
								NameSurname = "usuario1 usuario"
							}
						}
					}
				}, 1, 2024, 1
			},
			new object[] 
			{ 
				new List<DayGuard>()
				{
					new DayGuard
					{
						Id = 1,
						Day = new DateOnly(2024, 01, 01),
						assignedUsers = new List<User>()
						{
							new User()
							{
								Id = 1,
								Name = "usuario1",
								IdCenter = 1,
								IdLevel = 1,
								IdSpecialty = 1,
								Surname = "usuario"
							}
						}
					},
					new DayGuard
					{
						Id = 1,
						Day = new DateOnly(2024, 02, 01),
						assignedUsers = new List<User>()
						{
							new User()
							{
								Id = 1,
								Name = "usuario1",
								IdCenter = 1,
								IdLevel = 1,
								IdSpecialty = 1,
								Surname = "usuario"
							}
						}
					}
				},
				new List<DayGuardModel>()
				{
					new DayGuardModel
					{
						Id = 1,
						Day = new DateOnly(2024, 01, 01),
						assignedUsers = new List<UserModel>()
						{
							new UserModel()
							{
								Id = 1,
								NameSurname = "usuario1 usuario"
							}
						}
					},
					new DayGuardModel
					{
						Id = 1,
						Day = new DateOnly(2024, 02, 01),
						assignedUsers = new List<UserModel>()
						{
							new UserModel()
							{
								Id = 1,
								NameSurname = "usuario1 usuario"
							}
						}
					}
				},1, 2024, 0
			}
		};
	}
}
