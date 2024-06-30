using Castle.Components.DictionaryAdapter.Xml;
using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Services
{
	public class UserServiceTest
	{
		private Mock<IUserRepository<User>> _userRepository;
		private Mock<IPublicHolidayRepository<PublicHoliday>> _publicHolidayRepository;
		private Mock<IRolRepository<Rol>> _rolRepository;
		private Mock<ILevelRepository<Level>> _levelRepository;
		private Mock<ISpecialtyRepository<Specialty>> _specialtyRepository;
		private Mock<IUnityRepository<Unity>> _unityRepository;
		private IUserService _serviceUser;
		private IPublicHolidayService _servicePublicHoliday;

		[SetUp]
		public void Setup()
		{
			_userRepository = new Mock<IUserRepository<User>>();
			_publicHolidayRepository = new Mock<IPublicHolidayRepository<PublicHoliday>>();
			_rolRepository = new Mock<IRolRepository<Rol>>();
			_levelRepository = new Mock<ILevelRepository<Level>>();
			_specialtyRepository = new Mock<ISpecialtyRepository<Specialty>>();
			_unityRepository = new Mock<IUnityRepository<Unity>>();
			_servicePublicHoliday = new PublicHolidayService(_publicHolidayRepository.Object);
			_serviceUser = new UserService(_userRepository.Object, _servicePublicHoliday,
										   new RolService(_rolRepository.Object),
										   new LevelService(_levelRepository.Object),
										   new SpecialtyService(_specialtyRepository.Object),
										   new UnityService(_unityRepository.Object));
		}
	
		[TestCaseSource(nameof(GetUserByEmailCase))]
		[Test]
		public void UserServiceTestGetUserByEmailAndPassFindUser(UserModel? expected, User userReturned)
		{
			#region Arrange
			UserLogginRequest user = new UserLogginRequest()
			{
				Email = "usuario.usuario@salud.madrid.org",
				Password = "pasword"
			};

			_userRepository.Setup(ur => ur.GetUserByEmailAndPass(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(userReturned);

			_publicHolidayRepository.Setup(phr => phr.GetAllPublicHolidaysByCenter(It.IsAny<int>())).ReturnsAsync(new List<PublicHoliday>()
																									{
																										new PublicHoliday()
																										{
																											Id = 1,
																											IdType = 1,
																											Date = new DateOnly(2024, 1,1),
																											IdTypeNavigation = new PublicHolidayType()
																											{
																												Id = 1,
																												Description = "Regional"
																											}
																										} });

			#endregion

			#region Actual
			UserModel? actual = _serviceUser.GetUserByEmailAndPass(user.Email, user.Password).Result;
			#endregion

			#region Assert
			if (expected != null)
			{
				Assert.IsNotNull(actual);
				Assert.That(actual.Id, Is.EqualTo(expected.Id));
				Assert.That(actual.Email, Is.EqualTo(expected.Email));
				Assert.That(actual.centerId, Is.EqualTo(expected.centerId));
				Assert.That(actual.levelName, Is.EqualTo(expected.levelName));
				Assert.That(actual.rolName, Is.EqualTo(expected.rolName));
				Assert.That(actual.specialtyName, Is.EqualTo(expected.specialtyName));
				Assert.That(actual.centerName, Is.EqualTo(expected.centerName));
				Assert.That(actual.unityName, Is.EqualTo(expected.unityName));
				Assert.That(actual.NameSurname, Is.EqualTo(expected.NameSurname));
			}
			else
			{
				Assert.IsNull(actual);
			}
			#endregion
		}
		
		[Test]
		public void UserServiceTestGetUserByEmailAndPassFindUserException()
		{
			#region Arrange
			string user = "usuario.usuario@salud.madrid.org";
			string password = "pasword";

			_userRepository.Setup(ur => ur.GetUserByEmailAndPass(It.IsAny<string>(), It.IsAny<string>())).Throws(() => new Exception());
			#endregion

			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceUser.GetUserByEmailAndPass(It.IsAny<string>(), It.IsAny<string>()));
			#endregion
		}

		[Test]
		public void UserServiceTestGetAllUserByCenter()
		{
			#region Arrange
			List<UserModel> expected = new List<UserModel>(){
				new UserModel()
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					NameSurname = "usuario usuario",
					centerId = 1,
					centerName = "Centro",
					levelName = "Level",
					rolName = "Rol",
					specialtyName = "Especialidad",
					unityName = "Unidad",
					HolidayCurrentPeriod = 0,
					HolidayPreviousPeriod = 0,
					AskedHolidays = new List<AskedHolidayModel>()
				},
				new UserModel()
				{
					Id = 2,
					Email = "usuario2.usuario2@salud.madrid.org",
					NameSurname = "usuario2 usuario2",
					centerId = 1,
					centerName = "Centro",
					levelName = "Level",
					rolName = "Rol",
					specialtyName = "Especialidad",
					unityName = "Unidad",
					HolidayCurrentPeriod = 0,
					HolidayPreviousPeriod = 0,
					AskedHolidays = new List<AskedHolidayModel>()
				}
			};

			_userRepository.Setup(ur => ur.GetAllUsersByCenter(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new List<User>()
																						{
																							new User()
																							{
																								Id = 1,
																								Email = "usuario.usuario@salud.madrid.org",
																								IdCenter = 1,
																								IdCenterNavigation = new Center()
																								{
																									Name = "Centro"
																								},
																								IdLevel = 1,
																								IdLevelNavigation = new Level()
																								{
																									Name = "Level"
																								},
																								IdRol = 1,
																								IdRolNavigation = new Rol()
																								{
																									Name = "Rol"
																								},
																								IdUnity = 1,
																								IdUnityNavigation = new Unity()
																								{
																									Name = "Unidad"
																								},
																								IdSpecialty = 1,
																								IdSpecialtyNavigation = new Specialty()
																								{
																									Name = "Especialidad"
																								},
																								Name = "usuario",
																								Password = "",
																								Surname = "usuario",
																								HolidayCurrentPeridod = 0,
																								HolidayPreviousPeriod = 0
																							},
																							new User()
																							{
																								Id = 2,
																								Email = "usuario2.usuario2@salud.madrid.org",
																								IdCenter = 1,
																								IdCenterNavigation = new Center()
																								{
																									Name = "Centro"
																								},
																								IdLevel = 1,
																								IdLevelNavigation = new Level()
																								{
																									Name = "Level"
																								},
																								IdRol = 1,
																								IdRolNavigation = new Rol()
																								{
																									Name = "Rol"
																								},
																								IdUnity = 1,
																								IdUnityNavigation = new Unity()
																								{
																									Name = "Unidad"
																								},
																								IdSpecialty = 1,
																								IdSpecialtyNavigation = new Specialty()
																								{
																									Name = "Especialidad"
																								},
																								Name = "usuario2",
																								Password = "",
																								Surname = "usuario2",
																								HolidayCurrentPeridod = 0,
																								HolidayPreviousPeriod = 0
																							}
																						});

			_publicHolidayRepository.Setup(phr => phr.GetAllPublicHolidaysByCenter(It.IsAny<int>())).ReturnsAsync(new List<PublicHoliday>());
			#endregion

			#region Actual
			List<UserModel> actual = _serviceUser.GetAllUsersByCenter(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				CollectionAssert.AreEqual(expected[i].AskedHolidays, actual[i].AskedHolidays);
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].NameSurname, Is.EqualTo(expected[i].NameSurname));
				Assert.That(actual[i].centerName, Is.EqualTo(expected[i].centerName));
				Assert.That(actual[i].centerId, Is.EqualTo(expected[i].centerId));
				Assert.That(actual[i].Email, Is.EqualTo(expected[i].Email));
				Assert.That(actual[i].HolidayPreviousPeriod, Is.EqualTo(expected[i].HolidayPreviousPeriod));
				Assert.That(actual[i].levelName, Is.EqualTo(expected[i].levelName));
				Assert.That(actual[i].rolName, Is.EqualTo(expected[i].rolName));
				Assert.That(actual[i].specialtyName, Is.EqualTo(expected[i].specialtyName));
				Assert.That(actual[i].unityName, Is.EqualTo(expected[i].unityName));
				Assert.That(actual[i].HolidayCurrentPeriod, Is.EqualTo(expected[i].HolidayCurrentPeriod));
			}
			#endregion
		}
		
		[Test]
		public void UserServiceTestGetAllUserByCenterException()
		{
			#region Arrange
			_userRepository.Setup(ur => ur.GetAllUsersByCenter(It.IsAny<int>(), It.IsAny<bool>())).Throws(() => new Exception());
			#endregion

			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceUser.GetAllUsersByCenter(It.IsAny<int>()));
			#endregion
		}

		[TestCaseSource(nameof(GetUserCase))]
		[Test]
		public void UserServiceTestGetUserById(RealUserModel? expected, User? user)
		{
			#region Arrange
			_userRepository.Setup(ur => ur.GetUserById(It.IsAny<int>())).ReturnsAsync(user);
			#endregion

			#region Actual
			RealUserModel? actual = _serviceUser.GetUserById(1).Result;
			#endregion

			#region Assert
			if (expected != null)
			{
				Assert.IsNotNull(actual);
				Assert.That(actual.Id, Is.EqualTo(expected.Id));
				Assert.That(actual.Name, Is.EqualTo(expected.Name));
				Assert.That(actual.Surname, Is.EqualTo(expected.Surname));
				Assert.That(actual.Password, Is.EqualTo(expected.Password));
				Assert.That(actual.Email, Is.EqualTo(expected.Email));
				Assert.That(actual.LevelId, Is.EqualTo(expected.LevelId));
				Assert.That(actual.CenterId, Is.EqualTo(expected.CenterId));
				Assert.That(actual.RolId, Is.EqualTo(expected.RolId));
				Assert.That(actual.SpecialtyId, Is.EqualTo(expected.SpecialtyId));
			}
			else
			{
				Assert.IsNull(actual);
			}
			#endregion
		}

		[Test]
		public void UserServiceTestGetUserByIdException()
		{
			#region Arrange
			_userRepository.Setup(ur => ur.GetUserById(It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceUser.GetUserById(It.IsAny<int>()));
			#endregion
		}

		[Test]
		public void UserServiceTestGetUserModelById()
		{
			#region Arrange
			UserModel? expected = new UserModel()
			{
				Id = 1,
				Email = "usuario.usuario@salud.madrid.org",
				NameSurname  = "usuario usuario",
				centerId = 1,
				levelName = "Level",
				rolName = "Rol",
				specialtyName = "Especialidad",
				unityName = "Unidad",
				centerName = "Centro",
				Color = String.Empty,
				HolidayCurrentPeriod = 0,
				HolidayPreviousPeriod = 0
			};

			_userRepository.Setup(ur => ur.GetUserById(It.IsAny<int>())).ReturnsAsync(new User()
														{
															Id = 1,
															Email = "usuario.usuario@salud.madrid.org",
															IdCenter = 1,
															IdCenterNavigation = new Center()
															{
																Name = "Centro"
															},
															IdLevel = 1,
															IdLevelNavigation = new Level()
															{
																Name = "Level"
															},
															IdRol = 1,
															IdRolNavigation = new Rol()
															{
																Name = "Rol"
															},
															IdUnity = 1,
															IdUnityNavigation = new Unity()
															{
																Name = "Unidad"
															},
															IdSpecialty = 1,
															IdSpecialtyNavigation = new Specialty()
															{
																Name = "Especialidad"
															},
															Name = "usuario",
															Password = "",
															Surname = "usuario",
															HolidayCurrentPeridod = 0,
															HolidayPreviousPeriod = 0,
															AskedHolidays = new List<AskedHoliday>()
															{
																new AskedHoliday()
																{
																	DateFrom = new DateOnly(2024, 5, 5),
																	DateTo = new DateOnly(2024, 5, 10),
																	IdStatus = 1,
																	IdUser = 1,
																	Period = "2024",
																	IdStatusNavigation =  new HolidayStatus()
																	{
																		Description = "Solicitada"
																	}
																}
															}
														});

			_publicHolidayRepository.Setup(phr => phr.GetAllPublicHolidaysByCenter(It.IsAny<int>())).ReturnsAsync(new List<PublicHoliday>());
			#endregion

			#region Actual
			UserModel? actual = _serviceUser.GetUserModelById(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Email, Is.EqualTo(expected.Email));
			Assert.That(actual.NameSurname, Is.EqualTo(expected.NameSurname));
			Assert.That(actual.centerId, Is.EqualTo(expected.centerId));
			Assert.That(actual.levelName, Is.EqualTo(expected.levelName));
			Assert.That(actual.rolName, Is.EqualTo(expected.rolName));
			Assert.That(actual.specialtyName, Is.EqualTo(expected.specialtyName));
			Assert.That(actual.unityName, Is.EqualTo(expected.unityName));
			Assert.That(actual.centerName, Is.EqualTo(expected.centerName));
			Assert.That(actual.Color, Is.EqualTo(expected.Color));
			Assert.That(actual.HolidayCurrentPeriod, Is.EqualTo(expected.HolidayCurrentPeriod));
			Assert.That(actual.HolidayCurrentPeriod, Is.EqualTo(expected.HolidayCurrentPeriod));
			#endregion
		}
		
		[Test]
		public void UserServiceTestGetUserModelByIdException()
		{
			#region Arrange
			_userRepository.Setup(ur => ur.GetUserById(It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceUser.GetUserModelById(It.IsAny<int>()));
			#endregion
		}

		[Test]
		public void UserServiceTestGetAllUsersBySpecialty()
		{
			#region Arrange
			List<UserModel> expected = new List<UserModel>()
			{
				new UserModel()
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					NameSurname = "usuario usuario",
					centerId = 1,
					centerName = "Centro",
					levelName = "Level",
					rolName = "Rol",
					specialtyName = "Especialidad",
					unityName = "Unidad",
					HolidayCurrentPeriod = 0,
					HolidayPreviousPeriod = 0,
					AskedHolidays = new List<AskedHolidayModel>()
				}
			};

			_userRepository.Setup(ur => ur.GetAllUsersBySpecialty(It.IsAny<int>())).ReturnsAsync(new List<User>() {new User()
																					{
																						Id = 1,
																						Email = "usuario.usuario@salud.madrid.org",
																						IdCenter = 1,
																						IdCenterNavigation = new Center()
																						{
																							Name = "Centro"
																						},
																						IdLevel = 1,
																						IdLevelNavigation = new Level()
																						{
																							Name = "Level"
																						},
																						IdRol = 1,
																						IdRolNavigation = new Rol()
																						{
																							Name = "Rol"
																						},
																						IdUnity = 1,
																						IdUnityNavigation = new Unity()
																						{
																							Name = "Unidad"
																						},
																						IdSpecialty = 1,
																						IdSpecialtyNavigation = new Specialty()
																						{
																							Name = "Especialidad"
																						},
																						Name = "usuario",
																						Password = "",
																						Surname = "usuario",
																						HolidayCurrentPeridod = 0,
																						HolidayPreviousPeriod = 0
																					} });
			
			_publicHolidayRepository.Setup(phr => phr.GetAllPublicHolidaysByCenter(It.IsAny<int>())).ReturnsAsync(new List<PublicHoliday>());

			#endregion

			#region Actual
			List<UserModel> actual = _serviceUser.GetAllUsersBySpecialty(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				CollectionAssert.AreEqual(expected[i].AskedHolidays, actual[i].AskedHolidays);
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].NameSurname, Is.EqualTo(expected[i].NameSurname));
				Assert.That(actual[i].centerName, Is.EqualTo(expected[i].centerName));
				Assert.That(actual[i].centerId, Is.EqualTo(expected[i].centerId));
				Assert.That(actual[i].Email, Is.EqualTo(expected[i].Email));
				Assert.That(actual[i].HolidayPreviousPeriod, Is.EqualTo(expected[i].HolidayPreviousPeriod));
				Assert.That(actual[i].levelName, Is.EqualTo(expected[i].levelName));
				Assert.That(actual[i].rolName, Is.EqualTo(expected[i].rolName));
				Assert.That(actual[i].specialtyName, Is.EqualTo(expected[i].specialtyName));
				Assert.That(actual[i].unityName, Is.EqualTo(expected[i].unityName));
				Assert.That(actual[i].HolidayCurrentPeriod, Is.EqualTo(expected[i].HolidayCurrentPeriod));
			}
			#endregion
		}
		
		[Test]
		public void UserServiceTestGetAllUsersBySpecialtyException()
		{
			#region Arrange
			_userRepository.Setup(ur => ur.GetAllUsersBySpecialty(It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceUser.GetAllUsersBySpecialty(It.IsAny<int>()));
			#endregion
		}

		[TestCaseSource(nameof(GetAddUserCase))]
		[Test]
		public void UserServiceTestAddUser(User newUser, bool expected)
		{
			#region Arrange
			_userRepository.Setup(ur => ur.AddUser(It.IsAny<User>())).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceUser.AddUser(newUser).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void UserServiceTestAddUserException()
		{
			#region Assert
			Assert.ThrowsAsync<NullReferenceException>(async() => await _serviceUser.AddUser(It.IsAny<User>()));
			#endregion
		}

		[TestCaseSource(nameof(GetAddUsersCase))]
		[Test]
		public void UserServiceTestAddUsers(bool expected, string documento, Level? level, 
											Rol? rol, Specialty? specialty, Unity? unity)
		{
			#region Arrange
			_userRepository.Setup(ur => ur.AddUsers(It.IsAny<List<User>>())).ReturnsAsync(expected);
			_levelRepository.Setup(phr => phr.GetLevelByName(It.IsAny<string>())).ReturnsAsync(level);
			_rolRepository.Setup(phr => phr.GetRolByName(It.IsAny<string>())).ReturnsAsync(rol);
			_specialtyRepository.Setup(phr => phr.GetSpecialtyByName(It.IsAny<string>())).ReturnsAsync(specialty);
			_unityRepository.Setup(phr => phr.GetUnityByName(It.IsAny<string>())).ReturnsAsync(unity);
			#endregion

			#region Actual

			bool actual = _serviceUser.AddUsers(new StreamReader(documento), 1).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void UserServiceTestAddUsersException()
		{
			#region Assert
			Assert.ThrowsAsync<NullReferenceException>(async() => await _serviceUser.AddUsers(It.IsAny<StreamReader>(), It.IsAny<int>()));
			#endregion
		}

		[TestCaseSource(nameof(GetUpdateUserCase))]
		[Test]
		public void UserServiceTestUpdateUser(User updateUser, bool expected)
		{
			#region Arrange
			_userRepository.Setup(ur => ur.UpdateUser(It.IsAny<User>())).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceUser.UpdateUser(updateUser).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void UserServiceTestUpdateUserException()
		{
			#region Arrange
			User updateUser = new User()
			{
				Id = 1,
				Email = "usuario.usuario@salud.madrid.org",
				IdCenter = 0,
				IdLevel = 0,
				IdRol = 0,
				Name = "usuario",
				Password = "Password",
				Surname = "usuario"
			};
			_userRepository.Setup(ur => ur.UpdateUser(It.IsAny<User>())).Throws(() => new Exception());
			#endregion
			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceUser.UpdateUser(updateUser));
			#endregion
		}

		[TestCaseSource(nameof(GetDeleteUserCase))]
		[Test]
		public void UserServiceTestDeleteUser(int idUser, bool expected)
		{
			#region Arrange
			_userRepository.Setup(ur => ur.DeleteUser(It.IsAny<int>())).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceUser.DeleteUser(idUser).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void UserServiceTestDeleteUserException()
		{
			#region Arrange
			_userRepository.Setup(ur => ur.DeleteUser(It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceUser.DeleteUser(1));
			#endregion
		}

		private static object?[] GetUserByEmailCase =
		{
			new object[]
			{
				new UserModel()
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					centerId = 1,
					levelName = "level1",
					rolName = "rol1",
					specialtyName = "especialidad1",
					centerName = "Centro",
					unityName = "Unidad",
					NameSurname = "usuario usuario",

				},
				new User()
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					Name = "usuario",
					Surname = "usuario",
					IdCenter = 1,
					IdCenterNavigation = new Center()
					{
						Id = 1,
						Name = "Centro"
					},
					IdLevel = 1,
					IdLevelNavigation = new Level()
					{
						Id = 1,
						Name = "level1"
					},
					IdRol = 1,
					IdRolNavigation = new Rol()
					{
						Id = 1,
						Name = "rol1"
					},
					IdSpecialty = 1,
					IdSpecialtyNavigation = new Specialty()
					{
						Id = 1,
						Name = "especialidad1"
					},
					IdUnity = 1,
					IdUnityNavigation = new Unity()
					{
						Id = 1,
						Name = "Unidad"
					}
				}
			},
			new object[]
			{
				null,
				new User()
				{
					Id = 0,
					Email = "usuario.usuario@salud.madrid.org",
					Name = "usuario",
					Surname = "usuario",
					IdCenter = 1,
					IdCenterNavigation = new Center()
					{
						Id = 1,
						Name = "Centro"
					},
					IdLevel = 1,
					IdLevelNavigation = new Level()
					{
						Id = 1,
						Name = "level1"
					},
					IdRol = 1,
					IdRolNavigation = new Rol()
					{
						Id = 1,
						Name = "rol1"
					},
					IdSpecialty = 1,
					IdSpecialtyNavigation = new Specialty()
					{
						Id = 1,
						Name = "especialidad1"
					},
					IdUnity = 1,
					IdUnityNavigation = new Unity()
					{
						Id = 1,
						Name = "Unidad"
					}
				}
			},
			new object[]
			{
				null,
				null
			}
		};

		private static object?[] GetUserCase =
		{
			new object[]
			{
				new RealUserModel()
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					Name = "usuario",
					Surname = "usuario",
					CenterId = 1,
					LevelId = 1,
					RolId = 1,
					SpecialtyId = 1,
					UnityId = 1,
					Password = ""
				},
				(new RealUserModel()
				{
					Id = 1,
					Email = "usuario.usuario@salud.madrid.org",
					Name = "usuario",
					Surname = "usuario",
					CenterId = 1,
					LevelId = 1,
					RolId = 1,
					SpecialtyId = 1,
					UnityId = 1,
					Password = ""
				}).Map()
			},
			new object?[]
			{
				null,
				(new RealUserModel()
				{
					Id = 0,
					Email = "usuario.usuario@salud.madrid.org",
					Name = "usuario",
					Surname = "usuario",
					CenterId = 1,
					LevelId = 1,
					RolId = 1,
					SpecialtyId = 1,
					UnityId = 1,
					Password = ""
				}).Map()
			},
			new object?[]
			{
				null, null
			}
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
					Surname = "usuario5"}, true},
			new object[] { new User() {
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 1,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2
			}, false }
		};

		private static object[] GetAddUsersCase =
		{
			new object[] { true, "pruebaUser.csv",
				new Level()
				{
					Description = "level",
					Name = "level",
					Id = 1
				},
				new Rol()
				{
					Description = "rol",
					Name = "rol",
					Id = 1
				},
				new Specialty
				{
					Description = "specialty",
					Name = "specialty",
					MaxGuards = 6,
					Id = 1
				},
				new Unity
				{
					Description = "unidad",
					Name = "unidad",
					Id = 1
				}
			},
			new object?[] { false, "pruebaUser.csv",
				null,
				new Rol()
				{
					Description = "rol",
					Name = "rol",
					Id = 1
				},
				new Specialty
				{
					Description = "specialty",
					Name = "specialty",
					MaxGuards = 6,
					Id = 1
				},
				new Unity
				{
					Description = "unidad",
					Name = "unidad",
					Id = 1
				}
			},
			new object?[] { false, "pruebaUser.csv",
				new Level()
				{
					Description = "level",
					Name = "level",
					Id = 1
				},
				null,
				new Specialty
				{
					Description = "specialty",
					Name = "specialty",
					MaxGuards = 6,
					Id = 1
				},
				new Unity
				{
					Description = "unidad",
					Name = "unidad",
					Id = 1
				}
			},
			new object?[] { false, "pruebaUser.csv",
				new Level()
				{
					Description = "level",
					Name = "level",
					Id = 1
				},
				new Rol()
				{
					Description = "rol",
					Name = "rol",
					Id = 1
				},
				null,
				new Unity
				{
					Description = "unidad",
					Name = "unidad",
					Id = 1
				}
			},
			new object?[] { false, "pruebaUser.csv",
				new Level()
				{
					Description = "level",
					Name = "level",
					Id = 1
				},
				new Rol()
				{
					Description = "rol",
					Name = "rol",
					Id = 1
				},
				new Specialty
				{
					Description = "specialty",
					Name = "specialty",
					MaxGuards = 6,
					Id = 1
				},
				null
			},
			new object?[] { false, "pruebaUser.csv", null, null, null, null },
			new object?[] { false, "pruebaUserStringNull.csv", null, null, null, null }
		};

		private static object[] GetUpdateUserCase =
		{
			new object[] { new User() {
					Email = "usuario5.usuario5@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 2,
					IdSpecialty = 1,
					Name = "usuario5",
					Password = "password",
					Surname = "usuario5"}, false},
			new object[] { new User() {
					Email = "usuario.usuario@salud.madrid.org",
					IdCenter = 1,
					IdLevel = 1,
					IdRol = 1,
					Name = "usuario",
					Password = "password",
					Surname = "usuario",
					IdSpecialty = 2
			}, true }
		};

		private static object[] GetDeleteUserCase =
		{
			new object[] { 1, true},
			new object[] { 8, false }
		};
	}
}
