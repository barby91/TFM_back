using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Services
{
	public class UnityServiceTest
	{
		private Mock<IUnityRepository<Unity>> _unityRepository;
		private IUnityService _serviceUnity;

		[SetUp]
		public void Setup()
		{
			_unityRepository = new Mock<IUnityRepository<Unity>>();
			_serviceUnity = new UnityService(_unityRepository.Object);
		}

		[Test]
		public void UnityServiceTestGetAllCommonUnities()
		{
			#region expected
			List<UnityModel> expected = new List<UnityModel>()
			{
				new UnityModel
				{
					Id = 1,
					Name = "unidad1",
					Description = "unidad1"
				}
			};

			#endregion

			#region Arrange
			_unityRepository.Setup(ur => ur.GetAllCommonUnities(It.IsAny<int>())).ReturnsAsync(new List<Unity>()
																				{
																					new Unity
																					{
																						Id = 1,
																						Name = "unidad1",
																						Description = "unidad1"
																					}
																				});
			#endregion

			#region Actual
			List<UnityModel> actual = _serviceUnity.GetAllCommonUnities(It.IsAny<int>()).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].Description, Is.EqualTo(expected[i].Description));
			}
			#endregion
		}

		[Test]
		public void UnityServiceTestGetAllCommonUnitiesException()
		{
			#region Arrange
			_unityRepository.Setup(x => x.GetAllCommonUnities(It.IsAny<int>())).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceUnity.GetAllCommonUnities(It.IsAny<int>()));
		}
		
		[TestCaseSource(nameof(GetUnityByIdCase))]
		[Test]
		public void UnityServiceTestGetUnityById(UnityModel expected, Unity returnObject, bool isNull)
		{
			#region Arrange
			_unityRepository.Setup(ur => ur.GetUnityById(It.IsAny<int>())).ReturnsAsync(returnObject);
			#endregion

			#region Actual
			UnityModel? actual = _serviceUnity.GetUnityById(1).Result;
			#endregion

			#region Assert
			if (isNull)
			{
				Assert.IsNull(actual);
			}
			else
			{
				Assert.IsNotNull(actual);
				Assert.That(actual.Id, Is.EqualTo(expected.Id));
				Assert.That(actual.Name, Is.EqualTo(expected.Name));
				Assert.That(actual.Description, Is.EqualTo(expected.Description));
			}
			#endregion
		}

		[Test]
		public void UnityServiceTestGetUnityByIdException()
		{
			#region Arrange
			_unityRepository.Setup(x => x.GetUnityById(It.IsAny<int>())).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceUnity.GetUnityById(It.IsAny<int>()));
		}

		[Test]
		public void UnityyServiceTestGetlSpecialtyByName()
		{
			#region expected
			UnityModel expected = new UnityModel
			{
				Id = 1,
				Name = "unity1",
				Description = "unity1"
			};

			#endregion
			#region Arrange
			_unityRepository.Setup(ur => ur.GetUnityByName(It.IsAny<string>())).ReturnsAsync(new Unity
																					{
																						Id = 1,
																						Name = "unity1",
																						Description = "unity1"
																					});
			#endregion

			#region Actual
			UnityModel? actual = _serviceUnity.GetUnityByName("unity1").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			#endregion
		}

		[Test]
		public void UnityyServiceTestGetlSpecialtyByNameException()
		{
			#region Arrange
			_unityRepository.Setup(x => x.GetUnityByName(It.IsAny<string>())).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceUnity.GetUnityByName(It.IsAny<string>()));
		}


		[TestCaseSource(nameof(GetAddUnityCase))]
		[Test]
		public void UnityServiceTestAddUnity(Unity unity, bool expected)
		{
			#region Arrange
			_unityRepository.Setup(ur => ur.AddUnity(It.IsAny<Unity>())).ReturnsAsync(expected);
			#endregion

			bool actual = _serviceUnity.AddUnity(unity).Result;

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void UnityServiceTestAddUnityException()
		{
			#region Arrange
			_unityRepository.Setup(ur => ur.AddUnity(It.IsAny<Unity>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceUnity.AddUnity(It.IsAny<Unity>()));
		}

		[TestCaseSource(nameof(GetAddUnitiesCase))]
		[Test]
		public void UnityServiceTestAddUnities(bool expected, string documento)
		{
			#region Arrange
			_unityRepository.Setup(ur => ur.AddUnities(It.IsAny<List<Unity>>())).ReturnsAsync(expected);
			#endregion

			bool actual = _serviceUnity.AddUnities(new StreamReader(documento)).Result;

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void UnityServiceTestAddUnitiesException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _serviceUnity.AddUnities(It.IsAny<StreamReader>()));
		}

		[TestCaseSource(nameof(GetUpdateUnityCase))]
		[Test]
		public void UnityServiceTestUpdateUnity(Unity unity, bool expected)
		{
			#region Arrange		
			_unityRepository.Setup(ur => ur.UpdateUnity(It.IsAny<Unity>())).ReturnsAsync(expected);
			#endregion

			bool actual = _serviceUnity.UpdateUnity(unity).Result;

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void UnityServiceTestUpdateUnityException()
		{
			#region Arrange
			Unity unity = new Unity()
			{
				Id = 10,
				IdSpecialty = 1,
				Name = "unity10",
				Description = "unity10",
				MaxByDay = 2,
				MaxByDayWeekend = 3
			};

			_unityRepository.Setup(x => x.UpdateUnity(It.IsAny<Unity>())).Throws(() => new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _serviceUnity.UpdateUnity(unity));
		}

		[TestCaseSource(nameof(GetDeleteUnityCase))]
		[Test]
		public void UnityServiceTestDeleteUnity(int id, bool expected)
		{
			#region Arrange
			_unityRepository.Setup(ur => ur.DeleteUnity(It.IsAny<int>())).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceUnity.DeleteUnity(id).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void SpecialtyRepositoryTestDeleteSpecialtyException()
		{
			#region Arrange
			_unityRepository.Setup(x => x.DeleteUnity(It.IsAny<int>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceUnity.DeleteUnity(1));
		}

		private static object[] GetDeleteUnityCase =
		{
			new object[] { 1, true},
			new object[] { 4, false}
		};

		private static object[] GetUnityByIdCase =
		{
			new object[]
			{
				new UnityModel
				{
					Id = 1,
					Name = "unity1",
					Description = "unity1"
				},
				new Unity
				{
					Id = 1,
					Name = "unity1",
					Description = "unity1"
				},
				false
			},
			new object?[]
			{
				null,
				null,
				true
			},
			new object[]
			{
				new UnityModel
				{
					Id = 1,
					Name = "unity1",
					Description = "unity1"
				},
				new Unity
				{
					Id = 0,
					Name = "unity1",
					Description = "unity1"
				},
				true
			}
		};

		private static object[] GetAddUnityCase =
		{
			new object[] { new Unity() {
					IdSpecialty = 1,
					Name = "unidad5",
					Description = "unidad5"
			}, true},
			new object[] { new Unity() {
					IdSpecialty = 1,
					Name = "unidad1",
					Description = "unidad1"
			}, false}
		};

		private static object[] GetAddUnitiesCase =
		{
			new object[]
			{
				true, "pruebaUnity.csv"
			},
			new object[]
			{
				false, "pruebaUnity.csv"
			},
			new object[]
			{
				false, "pruebaUnityStringNull.csv"
			}
		};

		private static object[] GetUpdateUnityCase =
		{
			new object[] { new Unity() {
					Id = 1,
					IdSpecialty = 1,
					Name = "unidad1",
					Description = "unidad1"
			}, true},
			new object[] { new Unity() {
					Id = 8,
					IdSpecialty = 1,
					Name = "unidad8",
					Description = "unidad8"
			}, false}
		};
	}
}
