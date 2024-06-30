using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Services
{
	public class AskedHolidayServiceTest
	{
		private Mock<IAskedHolidayRepository<AskedHoliday>> _askedHolidayRepository;
		private Mock<IUserRepository<User>> _userRepository;
		private Mock<IPublicHolidayRepository<PublicHoliday>> _publicHolidayRepository;
		private Mock<IRolRepository<Rol>> _rolRepository;
		private Mock<ILevelRepository<Level>> _levelRepository;
		private Mock<ISpecialtyRepository<Specialty>> _specialtyRepository;
		private Mock<IUnityRepository<Unity>> _unityRepository;
		private IAskedHolidayService _serviceAskedHoliday;

		[SetUp]
		public void Setup()
		{
			_askedHolidayRepository = new Mock<IAskedHolidayRepository<AskedHoliday>>();
			_userRepository = new Mock<IUserRepository<User>>();
			_publicHolidayRepository = new Mock<IPublicHolidayRepository<PublicHoliday>>();
			_rolRepository = new Mock<IRolRepository<Rol>>();
			_levelRepository = new Mock<ILevelRepository<Level>>();
			_specialtyRepository = new Mock<ISpecialtyRepository<Specialty>>();
			_unityRepository = new Mock<IUnityRepository<Unity>>();
			_serviceAskedHoliday = new AskedHolidayService(_askedHolidayRepository.Object, 
														   new UserService(_userRepository.Object,
																		   new PublicHolidayService(_publicHolidayRepository.Object),
																		   new RolService(_rolRepository.Object),
																		   new LevelService(_levelRepository.Object),
																		   new SpecialtyService(_specialtyRepository.Object),
																		   new UnityService(_unityRepository.Object)
																		  ), 
														   new PublicHolidayService(_publicHolidayRepository.Object));
		}

		[TestCaseSource(nameof(GetPendingHolidaysCase))]
		[Test]
		public void AskedHolidayServiceTestGetAllPendingAskedHolidaysByCenter(int idCenter, int idUser, string type, 
																			  List<PendingAskedHolidayModel> expected, List<AskedHoliday> returnObject)
		{
			#region Arrange
			_askedHolidayRepository.Setup(ur => ur.GetAllPendingAskedHolidaysByCenter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(returnObject);
			#endregion

			#region Actual
			List<PendingAskedHolidayModel> actual = _serviceAskedHoliday.GetAllPendingAskedHolidaysByCenter(idCenter, idUser, type).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].DateTo, Is.EqualTo(expected[i].DateTo));
				Assert.That(actual[i].DateFrom, Is.EqualTo(expected[i].DateFrom));
				Assert.That(actual[i].IdUser, Is.EqualTo(expected[i].IdUser));
				Assert.That(actual[i].Period, Is.EqualTo(expected[i].Period));
			}
			#endregion
		}

		[Test]
		public void AskedHolidayServiceTestGetAllPendingAskedHolidaysByCenterException()
		{
			#region Arrange
			_askedHolidayRepository.Setup(ur => ur.GetAllPendingAskedHolidaysByCenter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceAskedHoliday.GetAllPendingAskedHolidaysByCenter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
		}

		[TestCaseSource(nameof(GetAddAskedHolidayCase))]
		[Test]
		public void AskedHolidayServiceTestAddAskedHoliday(AskedHolidayModel askedHoliday, bool expected, int idStatus)
		{
			#region Arrange
			_askedHolidayRepository.Setup(x => x.AddAskedHoliday(It.IsAny<AskedHoliday>())).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceAskedHoliday.AddAskedHoliday(askedHoliday.Map(), idStatus).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void AskedHolidayServiceTestAddAskedHolidayException()
		{
			#region Assert
			Assert.ThrowsAsync<NullReferenceException>(async() => await _serviceAskedHoliday.AddAskedHoliday(It.IsAny<AskedHoliday>(), It.IsAny<int>()));
			#endregion
		}

		[TestCaseSource(nameof(GetUpdateAskedHolidayCase))]
		[Test]
		public void AskedHolidayServiceTestUpdateAskedHoliday(AskedHoliday askedHoliday, bool expected, int idStatus)
		{
			#region Arrange
			_askedHolidayRepository.Setup(ur => ur.UpdateAskedHoliday(It.IsAny<AskedHoliday>())).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceAskedHoliday.UpdateAskedHoliday(askedHoliday, idStatus).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void AskedHolidayServiceTestUpdateAskedHolidayException()
		{
			#region Arrange
			AskedHoliday updateAskedHoliday = new AskedHoliday
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
			_askedHolidayRepository.Setup(ur => ur.UpdateAskedHoliday(It.IsAny<AskedHoliday>())).Throws(() => new Exception());
			#endregion
			#region Assert
			Assert.ThrowsAsync<Exception>(async() => await _serviceAskedHoliday.UpdateAskedHoliday(updateAskedHoliday, 1));
			#endregion
		}

		[TestCaseSource(nameof(GetCheckPendingHolidaysUser))]
		[Test]
		public void AskedHolidayServiceTestCheckPendingHolidaysUser(bool expected, User? resultObject, AskedHolidayModel askedHoliday)
		{
			#region Arrange
			_userRepository.Setup(ur => ur.GetUserById(It.IsAny<int>())).ReturnsAsync(resultObject);

			_publicHolidayRepository.Setup(phr => phr.GetAllPublicHolidaysByCenter(It.IsAny<int>())).ReturnsAsync(new List<PublicHoliday>());

			#endregion

			#region Actual
			bool actual = _serviceAskedHoliday.CheckPendingHolidaysUser(askedHoliday);
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void AskedHolidayServiceTestCheckPendingHolidaysUserException()
		{
			#region Assert
			Assert.Throws<NullReferenceException>(() => _serviceAskedHoliday.CheckPendingHolidaysUser(It.IsAny<AskedHolidayModel>()));
			#endregion
		}

		[Test]
		public void AskedHolidayServiceTestUpdateCancelAskedHoliday()
		{
			#region Arrange

			AskedHolidayModel askedHoliday = new AskedHolidayModel()
			{
				DateFrom = new DateOnly(2024, 5, 5),
				DateTo = new DateOnly(2024, 5, 15),
				Id = 0,
				IdUser = 1,
				Period = "2024",
				StatusDes = "Solicitado"
			};

			AskedHolidayModel expected = new AskedHolidayModel()
			{
				DateFrom = new DateOnly(2024, 5, 5),
				DateTo = new DateOnly(2024, 5, 15),
				Id = 1,
				IdUser = 1,
				Period = "2024",
				StatusDes = "Solicitado"
			};

			_askedHolidayRepository.Setup(phr => phr.GetPendingAskedHolidaysByDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
													.ReturnsAsync(new AskedHoliday
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
																	});

			#endregion

			#region Actual
			AskedHolidayModel? actual = _serviceAskedHoliday.UpdateCancelAskedHoliday(askedHoliday).Result;
			#endregion

			#region Assert

			Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.DateTo, Is.EqualTo(expected.DateTo));
			Assert.That(actual.DateFrom, Is.EqualTo(expected.DateFrom));
			Assert.That(actual.IdUser, Is.EqualTo(expected.IdUser));
			Assert.That(actual.Period, Is.EqualTo(expected.Period));

			#endregion
		}

		[Test]
		public void AskedHolidayServiceTestUpdateCancelAskedHolidayNull()
		{
			#region Arrange

			AskedHolidayModel askedHoliday = new AskedHolidayModel()
			{
				DateFrom = new DateOnly(2024, 5, 5),
				DateTo = new DateOnly(2024, 5, 15),
				Id = 0,
				IdUser = 1,
				Period = "2024",
				StatusDes = "Solicitado"
			};

			#endregion

			#region Actual
			AskedHolidayModel? actual = _serviceAskedHoliday.UpdateCancelAskedHoliday(askedHoliday).Result;
			#endregion

			#region Assert

			Assert.IsNull(actual);

			#endregion
		}

		[Test]
		public void AskedHolidayServiceTestUpdateCancelAskedHolidayException()
		{
			#region Assert
			Assert.ThrowsAsync<NullReferenceException>(async() => await _serviceAskedHoliday.UpdateCancelAskedHoliday(It.IsAny<AskedHolidayModel>()));
			#endregion
		}

		private static object[] GetPendingHolidaysCase =
		{
			new object[] {1, 2, "Solicitado", new List<PendingAskedHolidayModel>() {
					new PendingAskedHolidayModel
					{
						DateFrom = new DateOnly(2024, 5, 5),
						DateTo = new DateOnly(2024, 5, 15),
						Id = 1,
						NameSurname = "usuario usuario", 
						IdUser = 2,
						Period = "2024"
					}
				}, new List<AskedHoliday>() {
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
						IdUser = 2,
						IdUserNavigation = new User()
						{
							IdCenter = 1
						},
						Period = "2024"
					}
				}
			},
			new object[] { 1, 1, "Cancelado", new List<PendingAskedHolidayModel>() {
					new PendingAskedHolidayModel
					{
						DateFrom = new DateOnly(2024, 5, 5),
						DateTo = new DateOnly(2024, 5, 15),
						Id = 3,
						NameSurname = "usuario usuario",
						IdUser = 1,
						Period = "2024"
					}
				}, new List<AskedHoliday>() {
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
				1, 2, "Aprobado", new List<PendingAskedHolidayModel>()
				{
					new PendingAskedHolidayModel
					{
						DateFrom = new DateOnly(2024, 5, 5),
						DateTo = new DateOnly(2024, 5, 15),
						Id = 5,
						NameSurname = "usuario usuario",
						IdUser = 1,
						Period = "2024"
					}
				}, new List<AskedHoliday>()
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
				5, 1, "Aprobado", new List<PendingAskedHolidayModel>(), new List<AskedHoliday>()
			}
		};

		private static object[] GetCheckPendingHolidaysUser =
		{
			new object[]
			{
				true,
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
					HolidayCurrentPeridod = 22,
					HolidayPreviousPeriod = 22
				},
				new AskedHolidayModel()
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					StatusDes = "Solicitado",
					IdUser = 1,
					Period = "2024"
				}
			},
			new object[]
			{
				true,
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
					HolidayCurrentPeridod = 22,
					HolidayPreviousPeriod = 22
				},
				new AskedHolidayModel()
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					StatusDes = "Solicitado",
					IdUser = 1,
					Period = "2023"
				}
			},
			new object[]
			{
				true,
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
					HolidayCurrentPeridod = 22,
					HolidayPreviousPeriod = 22
				},
				new AskedHolidayModel()
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					StatusDes = "Solicitado",
					IdUser = 1,
					Period = "Weekend"
				}
			},
			new object?[]
			{
				false,
				null,
				new AskedHolidayModel()
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					StatusDes = "Cancelado",
					IdUser = 1,
					Period = "2024"
				}
			},
			new object[]
			{
				false,
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
					HolidayPreviousPeriod = 22
				},
				new AskedHolidayModel()
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					StatusDes = "Cancelado",
					IdUser = 1,
					Period = "2024"
				}
			},
			new object[]
			{
				false,
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
					HolidayCurrentPeridod = 22,
					HolidayPreviousPeriod = 0
				},
				new AskedHolidayModel()
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					StatusDes = "Cancelado",
					IdUser = 1,
					Period = "2023"
				}
			},
			new object[]
			{
				false,
				null,
				new AskedHolidayModel()
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					Id = 1,
					StatusDes = "Cancelado",
					IdUser = 1,
					Period = "2023"
				}
			}
		};

		private static object[] GetAddAskedHolidayCase =
		{
			new object[] { new AskedHolidayModel
				{
					DateFrom = new DateOnly(2024, 10, 5),
					DateTo = new DateOnly(2024, 10, 15),
					StatusDes = "Solicitado",
					IdUser = 1,
					Period = "2024"
				}, true, 1},
			new object[] { new AskedHolidayModel
				{
					DateFrom = new DateOnly(2024, 5, 5),
					DateTo = new DateOnly(2024, 5, 15),
					StatusDes = "Solicitado",
					IdUser = 1,
					Period = "2024"
				}, false, 1 }
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
				}, true, 1
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
				}, false, 1
			}
		};
	}
}
