using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Repository
{
	public class UserRepositoryTest
	{
		private IUserRepository<User> _userRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<User>>(x => x.Users)
				.ReturnsDbSet(GetFakeUsers());
			_userRepository = new UserRepository(dbContext.Object);
		}

		[Test]
		public void UserRepositoryTestGetUserByEmailAndPass()
		{
			#region expected

			User? expected = new User()
			{
				Id = 1,
				Email = "usuario.usuario@salud.madrid.org",
				IdCenter = 1,
				IdLevel = 1,
				IdRol = 2,
				Name = "usuario",
				Password = "",
				Surname = "usuario",
				IdSpecialty = 2,
				AskedHolidays = new List<AskedHoliday>()
				{
					new AskedHoliday()
					{
						DateFrom = new DateOnly(2024, 10, 20),
						DateTo = new DateOnly(2024, 10, 23),
						IdStatus = 1,
						IdUser = 1,
						Period = "2024",
						Id = 1
					}
				}
			};

			#endregion

			#region Actual
			User? actual = _userRepository.GetUserByEmailAndPass("usuario.usuario@salud.madrid.org", "password").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.AskedHolidays.Count, Is.EqualTo(expected.AskedHolidays.Count));
			for (int j = 0; j < actual.AskedHolidays.Count; j++)
			{
				Assert.That(actual.AskedHolidays.ToList()[j].DateFrom, Is.EqualTo(expected.AskedHolidays.ToList()[j].DateFrom));
				Assert.That(actual.AskedHolidays.ToList()[j].DateTo, Is.EqualTo(expected.AskedHolidays.ToList()[j].DateTo));
				Assert.That(actual.AskedHolidays.ToList()[j].IdStatus, Is.EqualTo(expected.AskedHolidays.ToList()[j].IdStatus));
				Assert.That(actual.AskedHolidays.ToList()[j].IdUser, Is.EqualTo(expected.AskedHolidays.ToList()[j].IdUser));
				Assert.That(actual.AskedHolidays.ToList()[j].Period, Is.EqualTo(expected.AskedHolidays.ToList()[j].Period));
				Assert.That(actual.AskedHolidays.ToList()[j].Id, Is.EqualTo(expected.AskedHolidays.ToList()[j].Id));
			}
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Surname, Is.EqualTo(expected.Surname));
			Assert.That(actual.Password, Is.EqualTo(expected.Password));
			Assert.That(actual.Email, Is.EqualTo(expected.Email));
			Assert.That(actual.HolidayCurrentPeridod, Is.EqualTo(expected.HolidayCurrentPeridod));
			Assert.That(actual.HolidayPreviousPeriod, Is.EqualTo(expected.HolidayPreviousPeriod));
			Assert.That(actual.IdLevel, Is.EqualTo(expected.IdLevel));
			Assert.That(actual.IdCenter, Is.EqualTo(expected.IdCenter));
			Assert.That(actual.IdRol, Is.EqualTo(expected.IdRol));
			Assert.That(actual.IdSpecialty, Is.EqualTo(expected.IdSpecialty));
			Assert.That(actual.IdLevelNavigation, Is.EqualTo(expected.IdLevelNavigation));
			Assert.That(actual.IdCenterNavigation, Is.EqualTo(expected.IdCenterNavigation));
			Assert.That(actual.IdRolNavigation, Is.EqualTo(expected.IdRolNavigation));
			Assert.That(actual.IdSpecialtyNavigation, Is.EqualTo(expected.IdSpecialtyNavigation));
			#endregion
		}

		[Test]
		public void UserRepositoryTestGetUserByEmailAndPassNull()
		{
			#region Actual
			User? actual = _userRepository.GetUserByEmailAndPass("nadie.usuario@salud.madrid.org", "password").Result;
			#endregion

			#region Assert
			Assert.IsNull(actual);
			#endregion
		}

		[Test]
		public void UserRepositoryTestGetUserByEmailAndPassException()
		{
			#region Arrange
			dbContext.Setup(x => x.Users).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _userRepository.GetUserByEmailAndPass(It.IsAny<string>(), It.IsAny<string>()));
		}

		[Test]
		public void UserRepositoryTestGetAllUsersByCenter()
		{
			#region expected
			List<User> expected = new List<User>()
			{
				new User
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 1,
							Period = "2024",
							Id = 1
						}
					}
				},
				new User
				{
					Id = 2,
					Email = "usuario2.usuario2@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 2,
					IdRol = 1,
					Name = "usuario2",
					Password = "password2",
					Surname = "usuario2",
					IdSpecialty = 1,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 2,
							Period = "2024",
							Id = 2
						}
					}
				}
			};
			#endregion

			#region Actual
			List<User> actual = _userRepository.GetAllUsersByCenter(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].AskedHolidays.Count, Is.EqualTo(expected[i].AskedHolidays.Count));
				for (int j = 0; j < actual[i].AskedHolidays.Count; j++)
				{
					Assert.That(actual[i].AskedHolidays.ToList()[j].DateFrom, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].DateFrom));
					Assert.That(actual[i].AskedHolidays.ToList()[j].DateTo, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].DateTo));
					Assert.That(actual[i].AskedHolidays.ToList()[j].IdStatus, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].IdStatus));
					Assert.That(actual[i].AskedHolidays.ToList()[j].IdUser, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].IdUser));
					Assert.That(actual[i].AskedHolidays.ToList()[j].Period, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].Period));
					Assert.That(actual[i].AskedHolidays.ToList()[j].Id, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].Id));
				}
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].Surname, Is.EqualTo(expected[i].Surname));
				Assert.That(actual[i].Password, Is.EqualTo(expected[i].Password));
				Assert.That(actual[i].Email, Is.EqualTo(expected[i].Email));
				Assert.That(actual[i].HolidayCurrentPeridod, Is.EqualTo(expected[i].HolidayCurrentPeridod));
				Assert.That(actual[i].HolidayPreviousPeriod, Is.EqualTo(expected[i].HolidayPreviousPeriod));
				Assert.That(actual[i].IdLevel, Is.EqualTo(expected[i].IdLevel));
				Assert.That(actual[i].IdCenter, Is.EqualTo(expected[i].IdCenter));
				Assert.That(actual[i].IdRol, Is.EqualTo(expected[i].IdRol));
				Assert.That(actual[i].IdSpecialty, Is.EqualTo(expected[i].IdSpecialty));
				Assert.That(actual[i].IdLevelNavigation, Is.EqualTo(expected[i].IdLevelNavigation));
				Assert.That(actual[i].IdCenterNavigation, Is.EqualTo(expected[i].IdCenterNavigation));
				Assert.That(actual[i].IdRolNavigation, Is.EqualTo(expected[i].IdRolNavigation));
				Assert.That(actual[i].IdSpecialtyNavigation, Is.EqualTo(expected[i].IdSpecialtyNavigation));
			}
			#endregion
		}

		[Test]
		public void UserRepositoryTestGetAllUsersWithHolidaysByCenter()
		{
			#region expected
			List<User> expected = new List<User>()
			{
				new User
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 1,
							Period = "2024",
							Id = 1
						}
					}
				},
				new User
				{
					Id = 2,
					Email = "usuario2.usuario2@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 2,
					IdRol = 1,
					Name = "usuario2",
					Password = "password2",
					Surname = "usuario2",
					IdSpecialty = 1,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 2,
							Period = "2024",
							Id = 2
						}
					}
				}
			};
			#endregion

			#region Actual
			List<User> actual = _userRepository.GetAllUsersByCenter(1, true).Result;
			#endregion

			#region Assert

			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].AskedHolidays.Count, Is.EqualTo(expected[i].AskedHolidays.Count));
				for (int j = 0; j < actual[i].AskedHolidays.Count; j++)
				{
					Assert.That(actual[i].AskedHolidays.ToList()[j].DateFrom, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].DateFrom));
					Assert.That(actual[i].AskedHolidays.ToList()[j].DateTo, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].DateTo));
					Assert.That(actual[i].AskedHolidays.ToList()[j].IdStatus, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].IdStatus));
					Assert.That(actual[i].AskedHolidays.ToList()[j].IdUser, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].IdUser));
					Assert.That(actual[i].AskedHolidays.ToList()[j].Period, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].Period));
					Assert.That(actual[i].AskedHolidays.ToList()[j].Id, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].Id));
				}
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].Surname, Is.EqualTo(expected[i].Surname));
				Assert.That(actual[i].Password, Is.EqualTo(expected[i].Password));
				Assert.That(actual[i].Email, Is.EqualTo(expected[i].Email));
				Assert.That(actual[i].HolidayCurrentPeridod, Is.EqualTo(expected[i].HolidayCurrentPeridod));
				Assert.That(actual[i].HolidayPreviousPeriod, Is.EqualTo(expected[i].HolidayPreviousPeriod));
				Assert.That(actual[i].IdLevel, Is.EqualTo(expected[i].IdLevel));
				Assert.That(actual[i].IdCenter, Is.EqualTo(expected[i].IdCenter));
				Assert.That(actual[i].IdRol, Is.EqualTo(expected[i].IdRol));
				Assert.That(actual[i].IdSpecialty, Is.EqualTo(expected[i].IdSpecialty));
				Assert.That(actual[i].IdLevelNavigation, Is.EqualTo(expected[i].IdLevelNavigation));
				Assert.That(actual[i].IdCenterNavigation, Is.EqualTo(expected[i].IdCenterNavigation));
				Assert.That(actual[i].IdRolNavigation, Is.EqualTo(expected[i].IdRolNavigation));
				Assert.That(actual[i].IdSpecialtyNavigation, Is.EqualTo(expected[i].IdSpecialtyNavigation));
			}
			#endregion
		}

		[Test]
		public void UserRepositoryTestGetAllUsersByCenterException()
		{
			#region Arrange
			dbContext.Setup(x => x.Users).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _userRepository.GetAllUsersByCenter(It.IsAny<int>()));
		}

		[Test]
		public void UserRepositoryTestGetUserById()
		{
			#region expected
			User expected = new User
			{
				Id = 1,
				Email = "usuario.usuario@salud.madrid.org",
				IdCenter = 1,
				IdLevel = 1,
				IdRol = 2,
				IdSpecialty = 2,
				Name = "usuario",
				Password = "password",
				Surname = "usuario",
				AskedHolidays = new List<AskedHoliday>()
				{
					new AskedHoliday()
					{
						DateFrom = new DateOnly(2024, 10, 20),
						DateTo = new DateOnly(2024, 10, 23),
						IdStatus = 1,
						IdUser = 1,
						Period = "2024",
						Id = 1
					}
				}
			};
			#endregion

			#region Actual
			User? actual = _userRepository.GetUserById(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.AskedHolidays.Count, Is.EqualTo(expected.AskedHolidays.Count));
			for (int j = 0; j < actual.AskedHolidays.Count; j++)
			{
				Assert.That(actual.AskedHolidays.ToList()[j].DateFrom, Is.EqualTo(expected.AskedHolidays.ToList()[j].DateFrom));
				Assert.That(actual.AskedHolidays.ToList()[j].DateTo, Is.EqualTo(expected.AskedHolidays.ToList()[j].DateTo));
				Assert.That(actual.AskedHolidays.ToList()[j].IdStatus, Is.EqualTo(expected.AskedHolidays.ToList()[j].IdStatus));
				Assert.That(actual.AskedHolidays.ToList()[j].IdUser, Is.EqualTo(expected.AskedHolidays.ToList()[j].IdUser));
				Assert.That(actual.AskedHolidays.ToList()[j].Period, Is.EqualTo(expected.AskedHolidays.ToList()[j].Period));
				Assert.That(actual.AskedHolidays.ToList()[j].Id, Is.EqualTo(expected.AskedHolidays.ToList()[j].Id));
			}
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Surname, Is.EqualTo(expected.Surname));
			Assert.That(actual.Password, Is.EqualTo(expected.Password));
			Assert.That(actual.Email, Is.EqualTo(expected.Email));
			Assert.That(actual.HolidayCurrentPeridod, Is.EqualTo(expected.HolidayCurrentPeridod));
			Assert.That(actual.HolidayPreviousPeriod, Is.EqualTo(expected.HolidayPreviousPeriod));
			Assert.That(actual.IdLevel, Is.EqualTo(expected.IdLevel));
			Assert.That(actual.IdCenter, Is.EqualTo(expected.IdCenter));
			Assert.That(actual.IdRol, Is.EqualTo(expected.IdRol));
			Assert.That(actual.IdSpecialty, Is.EqualTo(expected.IdSpecialty));
			Assert.That(actual.IdLevelNavigation, Is.EqualTo(expected.IdLevelNavigation));
			Assert.That(actual.IdCenterNavigation, Is.EqualTo(expected.IdCenterNavigation));
			Assert.That(actual.IdRolNavigation, Is.EqualTo(expected.IdRolNavigation));
			Assert.That(actual.IdSpecialtyNavigation, Is.EqualTo(expected.IdSpecialtyNavigation));
			#endregion
		}

		[Test]
		public void UserRepositoryTestGetUserByIdException()
		{
			#region Arrange
			dbContext.Setup(x => x.Users).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _userRepository.GetUserById(It.IsAny<int>()));
		}

		[TestCaseSource(nameof(GetSpecialtyUserCase))]
		[Test]
		public void UserRepositoryTestGetAllUsersBySpecialty(int idSpecialty, List<User> expected)
		{
			#region Actual
			List<User> actual = _userRepository.GetAllUsersBySpecialty(idSpecialty).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for(int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].AskedHolidays.Count, Is.EqualTo(expected[i].AskedHolidays.Count));
				for (int j = 0; j < actual[i].AskedHolidays.Count; j++)
				{
					Assert.That(actual[i].AskedHolidays.ToList()[j].DateFrom, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].DateFrom));
					Assert.That(actual[i].AskedHolidays.ToList()[j].DateTo, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].DateTo));
					Assert.That(actual[i].AskedHolidays.ToList()[j].IdStatus, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].IdStatus));
					Assert.That(actual[i].AskedHolidays.ToList()[j].IdUser, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].IdUser));
					Assert.That(actual[i].AskedHolidays.ToList()[j].Period, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].Period));
					Assert.That(actual[i].AskedHolidays.ToList()[j].Id, Is.EqualTo(expected[i].AskedHolidays.ToList()[j].Id));
				}
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].Surname, Is.EqualTo(expected[i].Surname));
				Assert.That(actual[i].Password, Is.EqualTo(expected[i].Password));
				Assert.That(actual[i].Email, Is.EqualTo(expected[i].Email));
				Assert.That(actual[i].HolidayCurrentPeridod, Is.EqualTo(expected[i].HolidayCurrentPeridod));
				Assert.That(actual[i].HolidayPreviousPeriod, Is.EqualTo(expected[i].HolidayPreviousPeriod));
				Assert.That(actual[i].IdLevel, Is.EqualTo(expected[i].IdLevel));
				Assert.That(actual[i].IdCenter, Is.EqualTo(expected[i].IdCenter));
				Assert.That(actual[i].IdRol, Is.EqualTo(expected[i].IdRol));
				Assert.That(actual[i].IdSpecialty, Is.EqualTo(expected[i].IdSpecialty));
				Assert.That(actual[i].IdLevelNavigation, Is.EqualTo(expected[i].IdLevelNavigation));
				Assert.That(actual[i].IdCenterNavigation, Is.EqualTo(expected[i].IdCenterNavigation));
				Assert.That(actual[i].IdRolNavigation, Is.EqualTo(expected[i].IdRolNavigation));
				Assert.That(actual[i].IdSpecialtyNavigation, Is.EqualTo(expected[i].IdSpecialtyNavigation));
			}
			#endregion
		}

		[Test]
		public void UserRepositoryTestGetAllUsersBySpecialtyException()
		{
			#region Arrange
			dbContext.Setup(x => x.Users).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _userRepository.GetAllUsersBySpecialty(It.IsAny<int>()));
		}

		[TestCaseSource(nameof(GetAddUserCase))]
		[Test]
		public void UserRepositoryTestAddUser(User user, bool expected, int expectedAddUser, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int addUser = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Users.AddAsync(It.IsAny<User>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addUser = callCount++);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_userRepository.AddUser(user);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Users.AddAsync(It.IsAny<User>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addUser, Is.EqualTo(expectedAddUser));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void UserRepositoryTestAddUserException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _userRepository.AddUser(It.IsAny<User>()));
		}

		[TestCaseSource(nameof(GetAddUsersCase))]
		[Test]
		public void UserRepositoryTestAddUsers(List<User> users, bool expected, int expectedAddUser, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int addUser = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Users.AddRangeAsync(It.IsAny<List<User>>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addUser = callCount++);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_userRepository.AddUsers(users);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Users.AddRangeAsync(It.IsAny<List<User>>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addUser, Is.EqualTo(expectedAddUser));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void UserRepositoryTestAddUsersException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _userRepository.AddUsers(It.IsAny<List<User>>()));
		}

		[TestCaseSource(nameof(GetUpdateUserCase))]
		[Test]
		public void UserRepositoryTestUpdateUser(User user, bool expected, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => saveChanges = callCount++);
			#endregion

			_userRepository.UpdateUser(user);

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
		public void UserRepositoryTestUpdateUserException()
		{
			#region Arrange

			User user = new User()
			{
				Id = 1,
				Email = "usuario5.usuario5@salud.madrid.org",
				IdCenter = 1,
				IdLevel = 1,
				IdRol = 2,
				IdSpecialty = 1,
				Name = "usuario5",
				Password = "password",
				Surname = "usuario5"
			};

			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _userRepository.UpdateUser(user));
		}

		[TestCaseSource(nameof(GetDeleteUserCase))]
		[Test]
		public void UserRepositoryTestDeleteUser(int id, bool expected, int expectedDeleteUser, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int deleteUser = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Users.Remove(It.IsAny<User>())).Callback(() => deleteUser = callCount++);
			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => saveChanges = callCount++);
			#endregion

			_userRepository.DeleteUser(id);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Users.Remove(It.IsAny<User>()), Times.Once());
				dbContext.Verify(x => x.SaveChangesAsync(default), Times.Once());
			}

			#region Assert
			Assert.That(deleteUser, Is.EqualTo(expectedDeleteUser));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void UserRepositoryTestDeleteUserException()
		{
			#region Arrange
			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _userRepository.DeleteUser(1));
		}

		private static object[] GetDeleteUserCase =
		{
			new object[] { 1, true, 0, 1},
			new object[] { 8, false, 0, 0 }
		};

		private static object[] GetAddUserCase =
		{
			new object[] { new User() {
					Email = "usuario5.usuario5@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					IdSpecialty = 1,
					Name = "usuario5",
					Password = "password",
					Surname = "usuario5"}, true, 0, 1},
			new object[] { new User() {
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 1,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2
			}, false, 0, 0 }
		};

		private static object[] GetAddUsersCase =
		{
			new object[] { new List<User>(){
				new User() {
					Email = "usuario5.usuario5@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					IdSpecialty = 1,
					Name = "usuario5",
					Password = "password",
					Surname = "usuario5"},
				new User() {
					Email = "usuario6.usuario6@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					IdSpecialty = 1,
					Name = "usuario6",
					Password = "password",
					Surname = "usuario6"},
				new User()
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 1,
							Period = "2024",
							Id = 1
						}
					}
				}
			}, true, 0, 1},
			new object[] { new List<User>(){
				new User() {
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 1,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2
				} 
			}, false, 0, 0 }
		};

		private static object[] GetUpdateUserCase =
		{
			new object[] { new User() {
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2}, true, 0},
			new object[] { new User() {
					Id=8,
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 1,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2
			}, false, 0}
		};

		private static object[] GetSpecialtyUserCase =
		{
			new object[] { 1, new List<User>(){
				new User
				{
					Id = 2,
					Email = "usuario2.usuario2@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 2,
					IdRol = 1,
					Name = "usuario2",
					Password = "password2",
					Surname = "usuario2",
					IdSpecialty = 1,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 2,
							Period = "2024",
							Id = 2
						}
					}
				},
				new User
				{
					Id = 4,
					Email = "usuario4.usuario4@salud.madrid.org",
					IdCenter = 2,
					IdLevel = 1,
					IdRol = 1,
					Name = "usuario4",
					Password = "password4",
					Surname = "usuario4",
					IdSpecialty = 1
				}
			}},
			new object[] { 2, new List<User>()
			{
				new User
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 1,
							Period = "2024",
							Id = 1
						}
					}
				},
				new User
				{
					Id = 3,
					Email = "usuario3.usuario3@salud.madrid.org",
					IdCenter = 2,
					IdLevel = 2,
					IdRol = 2,
					Name = "usuario3",
					Password = "password3",
					Surname = "usuario3",
					IdSpecialty = 2
				}
			}}
		};

		private List<User> GetFakeUsers()
		{
			return new List<User>()
			{
				new User
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2,
					AskedHolidays = new	List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 1,
							Period = "2024",
							Id = 1
						}
					}
				},
				new User
				{
					Id = 2,
					Email = "usuario2.usuario2@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 2,
					IdRol = 1,
					Name = "usuario2",
					Password = "password2",
					Surname = "usuario2",
					IdSpecialty = 1,
					AskedHolidays = new List<AskedHoliday>()
					{
						new AskedHoliday()
						{
							DateFrom = new DateOnly(2024, 10, 20),
							DateTo = new DateOnly(2024, 10, 23),
							IdStatus = 1,
							IdUser = 2,
							Period = "2024",
							Id = 2
						}
					}
				},
				new User
				{
					Id = 3,
					Email = "usuario3.usuario3@salud.madrid.org",
					IdCenter = 2,
					IdLevel = 2,
					IdRol = 2,
					Name = "usuario3",
					Password = "password3",
					Surname = "usuario3",
					IdSpecialty = 2
				},
				new User
				{
					Id = 4,
					Email = "usuario4.usuario4@salud.madrid.org",
					IdCenter = 2,
					IdLevel = 1,
					IdRol = 1,
					Name = "usuario4",
					Password = "password4",
					Surname = "usuario4",
					IdSpecialty = 1
				}
			};
		}
	}
}
