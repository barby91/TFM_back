using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
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
													_unityRepository.Object
												   );
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
		public void DayGuardServiceTestGetlDeletePreviousGuardException()
		{
			#region Arrange
			_dayGuardRepository.Setup(ur => ur.DeletePreviousGuard(It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceDayGuard.DeletePreviousGuard(1));
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
			_unityRepository.Setup(u => u.GetAllCommonUnities()).ReturnsAsync(GetFakeCommonUnities());
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
				new User{Id=7, Name="usuario", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 1, 2),DateTo = new DateOnly(2024, 1, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 7, 15),DateTo = new DateOnly(2024, 7, 26),IdStatus = 3}}},
				new User{Id=10008, Name="usuario2", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 1, 2),DateTo = new DateOnly(2024, 1, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 7, 15),DateTo = new DateOnly(2024, 7, 26),IdStatus = 3}}},
				new User{Id=10009, Name="usuario3", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 1, 10),DateTo = new DateOnly(2024, 1, 15),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 8, 15),DateTo = new DateOnly(2024, 8, 26),IdStatus = 3}}},
				new User{Id=10010, Name="usuario4", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 2, 2),DateTo = new DateOnly(2024, 2, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 6, 15),DateTo = new DateOnly(2024, 6, 26),IdStatus = 3}}},
				new User{Id=10011, Name="usuario5", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 3, 2),DateTo = new DateOnly(2024, 3, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 9, 15),DateTo = new DateOnly(2024, 9, 26),IdStatus = 3}}},
				new User{Id=10012, Name="usuario11", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 5, 2),DateTo = new DateOnly(2024, 5, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 10, 15),DateTo = new DateOnly(2024, 10, 26),IdStatus = 3}}},
				new User{Id=10013, Name="usuario12", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 11, 2),DateTo = new DateOnly(2024, 11, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 3, 15),DateTo = new DateOnly(2024, 3, 26),IdStatus = 3}}},
				new User{Id=10014, Name="usuario13", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 12, 2),DateTo = new DateOnly(2024, 12, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 2, 15),DateTo = new DateOnly(2024, 2, 26),IdStatus = 3}}},
				new User{Id=10015, Name="usuario14", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 7, 2),DateTo = new DateOnly(2024, 7, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 3, 15),DateTo = new DateOnly(2024, 3, 26),IdStatus = 3}}},
				new User{Id=10016, Name="usuario15", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 6, 2),DateTo = new DateOnly(2024, 6, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 4, 15),DateTo = new DateOnly(2024, 4, 26),IdStatus = 3}}},
				new User{Id=10017, Name="usuario21", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =1, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 6, 2),DateTo = new DateOnly(2024, 6, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 5, 15),DateTo = new DateOnly(2024, 5, 26),IdStatus = 3}}},
				new User{Id=10018, Name="usuario22", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =1, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 8, 20),DateTo = new DateOnly(2024, 8, 27),IdStatus = 3}}},
				new User{Id=10019, Name="usuario23", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =1},
				new User{Id=10020, Name="usuario24", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =1},
				new User{Id=10021, Name="usuario25", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =1},
				new User{Id=10022, Name="usuario26", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =4},
				new User{Id=10023, Name="usuario28", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =6},
				new User{Id=10024, Name="usuario30", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =1},
				new User{Id=10025, Name="usuario16", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =7},
				new User{Id=10026, Name="usuario17", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =7},
				new User{Id=10027, Name="usuario18", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =7},
				new User{Id=10028, Name="usuario19", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =7},
				new User{Id=10029, Name="usuario20", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =7},
				new User{Id=10030, Name="usuario29", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =7},
				new User{Id=10031, Name="usuario6", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =5},
				new User{Id=10032, Name="usuario7", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =5},
				new User{Id=10033, Name="usuario8", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =5},
				new User{Id=30020, Name="usuario9", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =5},
				new User{Id=30021, Name="usuario10", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =5},
				new User{Id=30022, Name="usuario27", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =5},
				new User{Id=30023, Name="usuario31", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =4},
				new User{Id=30036, Name="usuario32", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =5},
				new User{Id=30041, Name="usuario33", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =6},
				new User{Id=40041, Name="usuario34", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =7},
				new User{Id=40042, Name="usuario35", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =1}
			};
		}
		
		private static List<User> GetFake30UserWithHolidays()
		{
			return new List<User>()
			{
				new User{Id=7, Name="usuario", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 1, 2),DateTo = new DateOnly(2024, 1, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 7, 15),DateTo = new DateOnly(2024, 7, 26),IdStatus = 3}}},
				new User{Id=10008, Name="usuario2", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 1, 2),DateTo = new DateOnly(2024, 1, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 7, 15),DateTo = new DateOnly(2024, 7, 26),IdStatus = 3}}},
				new User{Id=10009, Name="usuario3", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 1, 10),DateTo = new DateOnly(2024, 1, 15),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 8, 15),DateTo = new DateOnly(2024, 8, 26),IdStatus = 3}}},
				new User{Id=10010, Name="usuario4", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 2, 2),DateTo = new DateOnly(2024, 2, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 6, 15),DateTo = new DateOnly(2024, 6, 26),IdStatus = 3}}},
				new User{Id=10011, Name="usuario5", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =4, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 3, 2),DateTo = new DateOnly(2024, 3, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 9, 15),DateTo = new DateOnly(2024, 9, 26),IdStatus = 3}}},
				new User{Id=10012, Name="usuario11", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 5, 2),DateTo = new DateOnly(2024, 5, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 10, 15),DateTo = new DateOnly(2024, 10, 26),IdStatus = 3}}},
				new User{Id=10013, Name="usuario12", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 11, 2),DateTo = new DateOnly(2024, 11, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 3, 15),DateTo = new DateOnly(2024, 3, 26),IdStatus = 3}}},
				new User{Id=10014, Name="usuario13", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 12, 2),DateTo = new DateOnly(2024, 12, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 2, 15),DateTo = new DateOnly(2024, 2, 26),IdStatus = 3}}},
				new User{Id=10015, Name="usuario14", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 7, 2),DateTo = new DateOnly(2024, 7, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 3, 15),DateTo = new DateOnly(2024, 3, 26),IdStatus = 3}}},
				new User{Id=10016, Name="usuario15", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =6, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 6, 2),DateTo = new DateOnly(2024, 6, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 4, 15),DateTo = new DateOnly(2024, 4, 26),IdStatus = 3}}},
				new User{Id=10017, Name="usuario21", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =1, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 6, 2),DateTo = new DateOnly(2024, 6, 7),IdStatus = 3},new AskedHoliday(){DateFrom = new DateOnly(2024, 5, 15),DateTo = new DateOnly(2024, 5, 26),IdStatus = 3}}},
				new User{Id=10018, Name="usuario22", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =1, AskedHolidays = new List<AskedHoliday>(){new AskedHoliday(){DateFrom = new DateOnly(2024, 8, 20),DateTo = new DateOnly(2024, 8, 27),IdStatus = 3}}},
				new User{Id=10019, Name="usuario23", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =1},
				new User{Id=10020, Name="usuario24", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =1},
				new User{Id=10021, Name="usuario25", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =1},
				new User{Id=10022, Name="usuario26", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =4},
				new User{Id=10023, Name="usuario28", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =6},
				new User{Id=10024, Name="usuario30", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =1},
				new User{Id=10025, Name="usuario16", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =7},
				new User{Id=10026, Name="usuario17", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =7},
				new User{Id=10027, Name="usuario18", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =7},
				new User{Id=10028, Name="usuario19", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =7},
				new User{Id=10029, Name="usuario20", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =7},
				new User{Id=10030, Name="usuario29", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =7},
				new User{Id=10031, Name="usuario6", Surname="fichero", IdLevel=1, IdLevelNavigation=new Level(){Id=1, Name="R1"},IdSpecialty=1, IdUnity =5},
				new User{Id=10032, Name="usuario7", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =5},
				new User{Id=10033, Name="usuario8", Surname="fichero", IdLevel=3, IdLevelNavigation=new Level(){Id=3, Name="R3"},IdSpecialty=1, IdUnity =5},
				new User{Id=30020, Name="usuario9", Surname="fichero", IdLevel=6, IdLevelNavigation=new Level(){Id=6, Name="R4"},IdSpecialty=1, IdUnity =5},
				new User{Id=30021, Name="usuario10", Surname="fichero", IdLevel=7, IdLevelNavigation=new Level(){Id=7, Name="R5"},IdSpecialty=1, IdUnity =5},
				new User{Id=30022, Name="usuario27", Surname="fichero", IdLevel=2, IdLevelNavigation=new Level(){Id=2, Name="R2"},IdSpecialty=1, IdUnity =5},
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
										Id = 4,
										Name = "Colon",
										IdSpecialty = 1
									},
									new Unity()
									{
										Id = 5,
										Name = "Hepáticobilio",
										IdSpecialty = 1
									},
									new Unity()
									{
										Id = 7,
										Name = "Esófago gástrica",
										IdSpecialty = 1
									},
									new Unity()
									{
										Id = 6,
										Name = "Endocrino",
										IdSpecialty = 1
									}
								}
				},
				new Specialty
				{
					Id = 2,
					MaxGuards = 6,
					Unities = new List<Unity>()
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
							Name = "Rotatorio"
						}
					};
		}

		private List<DayGuard> GetFakeDayGuards()
		{
			return new List<DayGuard>()
			{
				new DayGuard{Id=1, Day = new DateOnly(2024, 1,1), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=7, Day = new DateOnly(2024, 1,2), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=13, Day = new DateOnly(2024, 1,3), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=19, Day = new DateOnly(2024, 1,4), assignedUsers=new List<User>(){new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=25, Day = new DateOnly(2024, 1,5), assignedUsers=new List<User>(){new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=31, Day = new DateOnly(2024, 1,6), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=37, Day = new DateOnly(2024, 1,7), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=43, Day = new DateOnly(2024, 1,8), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=49, Day = new DateOnly(2024, 1,9), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=55, Day = new DateOnly(2024, 1,10), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=61, Day = new DateOnly(2024, 1,11), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=67, Day = new DateOnly(2024, 1,12), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=73, Day = new DateOnly(2024, 1,13), assignedUsers=new List<User>(){new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=79, Day = new DateOnly(2024, 1,14), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=85, Day = new DateOnly(2024, 1,15), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=91, Day = new DateOnly(2024, 1,16), assignedUsers=new List<User>(){new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=97, Day = new DateOnly(2024, 1,17), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=103, Day = new DateOnly(2024, 1,18), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=109, Day = new DateOnly(2024, 1,19), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=115, Day = new DateOnly(2024, 1,20), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=121, Day = new DateOnly(2024, 1,21), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=127, Day = new DateOnly(2024, 1,22), assignedUsers=new List<User>(){new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=133, Day = new DateOnly(2024, 1,23), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=139, Day = new DateOnly(2024, 1,24), assignedUsers=new List<User>(){new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=145, Day = new DateOnly(2024, 1,25), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=151, Day = new DateOnly(2024, 1,26), assignedUsers=new List<User>(){new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=157, Day = new DateOnly(2024, 1,27), assignedUsers=new List<User>(){new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=163, Day = new DateOnly(2024, 1,28), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=169, Day = new DateOnly(2024, 1,29), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=175, Day = new DateOnly(2024, 1,30), assignedUsers=new List<User>(){new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=181, Day = new DateOnly(2024, 1,31), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=181, Day = new DateOnly(2024, 4,1), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=187, Day = new DateOnly(2024, 4,2), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=193, Day = new DateOnly(2024, 4,3), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=199, Day = new DateOnly(2024, 4,4), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=205, Day = new DateOnly(2024, 4,5), assignedUsers=new List<User>(){new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=211, Day = new DateOnly(2024, 4,6), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=217, Day = new DateOnly(2024, 4,7), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=223, Day = new DateOnly(2024, 4,8), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=229, Day = new DateOnly(2024, 4,9), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=235, Day = new DateOnly(2024, 4,10), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=241, Day = new DateOnly(2024, 4,11), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=247, Day = new DateOnly(2024, 4,12), assignedUsers=new List<User>(){new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=253, Day = new DateOnly(2024, 4,13), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=259, Day = new DateOnly(2024, 4,14), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=265, Day = new DateOnly(2024, 4,15), assignedUsers=new List<User>(){new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=271, Day = new DateOnly(2024, 4,16), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=277, Day = new DateOnly(2024, 4,17), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=283, Day = new DateOnly(2024, 4,18), assignedUsers=new List<User>(){new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=289, Day = new DateOnly(2024, 4,19), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=295, Day = new DateOnly(2024, 4,20), assignedUsers=new List<User>(){new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=301, Day = new DateOnly(2024, 4,21), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=307, Day = new DateOnly(2024, 4,22), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=313, Day = new DateOnly(2024, 4,23), assignedUsers=new List<User>(){new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=319, Day = new DateOnly(2024, 4,24), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=325, Day = new DateOnly(2024, 4,25), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=331, Day = new DateOnly(2024, 4,26), assignedUsers=new List<User>(){new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=337, Day = new DateOnly(2024, 4,27), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=343, Day = new DateOnly(2024, 4,28), assignedUsers=new List<User>(){new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=349, Day = new DateOnly(2024, 4,29), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=355, Day = new DateOnly(2024, 4,30), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																								 new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=361, Day = new DateOnly(2024, 8,1), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=367, Day = new DateOnly(2024, 8,2), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
																												 new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=373, Day = new DateOnly(2024, 8,3), assignedUsers=new List<User>(){new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=379, Day = new DateOnly(2024, 8,4), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=385, Day = new DateOnly(2024, 8,5), assignedUsers=new List<User>(){new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=391, Day = new DateOnly(2024, 8,6), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=397, Day = new DateOnly(2024, 8,7), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=403, Day = new DateOnly(2024, 8,8), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=409, Day = new DateOnly(2024, 8,9), assignedUsers=new List<User>(){new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=415, Day = new DateOnly(2024, 8,10), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=421, Day = new DateOnly(2024, 8,11), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=427, Day = new DateOnly(2024, 8,12), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=433, Day = new DateOnly(2024, 8,13), assignedUsers=new List<User>(){new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=439, Day = new DateOnly(2024, 8,14), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=445, Day = new DateOnly(2024, 8,15), assignedUsers=new List<User>(){new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=451, Day = new DateOnly(2024, 8,16), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=457, Day = new DateOnly(2024, 8,17), assignedUsers=new List<User>(){new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=463, Day = new DateOnly(2024, 8,18), assignedUsers=new List<User>(){new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=469, Day = new DateOnly(2024, 8,19), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=475, Day = new DateOnly(2024, 8,20), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=481, Day = new DateOnly(2024, 8,21), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=487, Day = new DateOnly(2024, 8,22), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=493, Day = new DateOnly(2024, 8,23), assignedUsers=new List<User>(){new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=499, Day = new DateOnly(2024, 8,24), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=505, Day = new DateOnly(2024, 8,25), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=511, Day = new DateOnly(2024, 8,26), assignedUsers=new List<User>(){new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=517, Day = new DateOnly(2024, 8,27), assignedUsers=new List<User>(){new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=523, Day = new DateOnly(2024, 8,28), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=529, Day = new DateOnly(2024, 8,29), assignedUsers=new List<User>(){new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=535, Day = new DateOnly(2024, 8,30), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=540, Day = new DateOnly(2024, 8,31), assignedUsers=new List<User>(){new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=541, Day = new DateOnly(2024, 6,1), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=547, Day = new DateOnly(2024, 6,2), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=553, Day = new DateOnly(2024, 6,3), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=559, Day = new DateOnly(2024, 6,4), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=565, Day = new DateOnly(2024, 6,5), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=571, Day = new DateOnly(2024, 6,6), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=577, Day = new DateOnly(2024, 6,7), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=583, Day = new DateOnly(2024, 6,8), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=589, Day = new DateOnly(2024, 6,9), assignedUsers=new List<User>(){new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=595, Day = new DateOnly(2024, 6,10), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=601, Day = new DateOnly(2024, 6,11), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=607, Day = new DateOnly(2024, 6,12), assignedUsers=new List<User>(){new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=613, Day = new DateOnly(2024, 6,13), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=619, Day = new DateOnly(2024, 6,14), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=625, Day = new DateOnly(2024, 6,15), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=631, Day = new DateOnly(2024, 6,16), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=637, Day = new DateOnly(2024, 6,17), assignedUsers=new List<User>(){new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=643, Day = new DateOnly(2024, 6,18), assignedUsers=new List<User>(){new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=649, Day = new DateOnly(2024, 6,19), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=655, Day = new DateOnly(2024, 6,20), assignedUsers=new List<User>(){new User(){Id=10010, Name="usuario4", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=661, Day = new DateOnly(2024, 6,21), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=667, Day = new DateOnly(2024, 6,22), assignedUsers=new List<User>(){new User(){Id=10009, Name="usuario3", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=673, Day = new DateOnly(2024, 6,23), assignedUsers=new List<User>(){new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10020, Name="usuario24", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10022, Name="usuario26", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10023, Name="usuario28", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10026, Name="usuario17", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=679, Day = new DateOnly(2024, 6,24), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40042, Name="usuario35", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=685, Day = new DateOnly(2024, 6,25), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=691, Day = new DateOnly(2024, 6,26), assignedUsers=new List<User>(){new User(){Id=10011, Name="usuario5", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10014, Name="usuario13", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10017, Name="usuario21", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30036, Name="usuario32", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=697, Day = new DateOnly(2024, 6,27), assignedUsers=new List<User>(){new User(){Id=10008, Name="usuario2", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10016, Name="usuario15", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10018, Name="usuario22", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10019, Name="usuario23", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10031, Name="usuario6", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=40041, Name="usuario34", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=703, Day = new DateOnly(2024, 6,28), assignedUsers=new List<User>(){new User(){Id=10013, Name="usuario12", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10015, Name="usuario14", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10021, Name="usuario25", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10024, Name="usuario30", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10025, Name="usuario16", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10027, Name="usuario18", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=709, Day = new DateOnly(2024, 6,29), assignedUsers=new List<User>(){new User(){Id=10007, Name="usuario", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10028, Name="usuario19", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10030, Name="usuario29", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10032, Name="usuario7", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10033, Name="usuario8", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30021, Name="usuario10", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"}}},
				new DayGuard{Id=715, Day = new DateOnly(2024, 6,30), assignedUsers=new List<User>(){new User(){Id=10012, Name="usuario11", IdCenter = 1, IdLevel=1, IdSpecialty=1, Surname="fichero"},
				new User(){Id=10029, Name="usuario20", IdCenter = 1, IdLevel=7, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30020, Name="usuario9", IdCenter = 1, IdLevel=6, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30022, Name="usuario27", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30023, Name="usuario31", IdCenter = 1, IdLevel=2, IdSpecialty=1, Surname="fichero"},
				new User(){Id=30041, Name="usuario33", IdCenter = 1, IdLevel=3, IdSpecialty=1, Surname="fichero"}}},
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

		private static object[] GetUserStatsCase =
		{
			new object[] 
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 1
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 1
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 2
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 0
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 0
				}, "OK", GetFakeUserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 2
				}, "Error al guardar la guardia", GetFakeUserWithHolidays(), false, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 2
				}, "Error al borrar la guardia previamente calculada", GetFakeUserWithHolidays(), false, false
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 1
				}, "No se pueden asignar las guardias del mes", GetFake30UserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 1
				}, "No se pueden asignar las guardias del mes", GetFake30UserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 2
				}, "No se pueden asignar las guardias del mes", GetFake30UserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 0,
					month = 0
				}, "No se pueden asignar las guardias del mes", GetFake30UserWithHolidays(), true, true
			},
			new object[]
			{
				new GuardRequest()
				{
					idCenter = 1,
					idSpecialty = 1,
					month = 0
				}, "No se pueden asignar las guardias del mes", GetFake30UserWithHolidays(), true, true
			}
		};

		private static object[] GetDeleteGuardsCase =
		{
			new object[] {1, true},
			new object[] {0, true},
			new object[] {12, true},
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
