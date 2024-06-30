using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Repository
{
	public class UnityRepositoryTest
	{
		private IUnityRepository<Unity> _unityRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<Unity>>(x => x.Unities)
				.ReturnsDbSet(GetFakeUnities());
			_unityRepository = new UnityRepository(dbContext.Object);
		}

		[Test]
		public void UnityRepositoryTestGetAllCommonUnities()
		{
			#region arrange
			dbContext.Setup<DbSet<Unity>>(x => x.Unities)
				.ReturnsDbSet(GetFakeUnities());
			#endregion

			#region expected
			List<Unity> expected = new List<Unity>()
			{
				new Unity()
				{
					Id = 9,
					Description = "rotatorio",
					Name = "rotatorio"
				}
			};
			#endregion

			#region Actual
			List<Unity> actual = _unityRepository.GetAllCommonUnities().Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; i++)
			{
				CollectionAssert.AreEqual(expected[i].Users, actual[i].Users);
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].Description, Is.EqualTo(expected[i].Description));
				Assert.That(actual[i].IdSpecialty, Is.EqualTo(expected[i].IdSpecialty));
				Assert.That(actual[i].IdSpecialtyNavigation, Is.EqualTo(expected[i].IdSpecialtyNavigation));
			}
			#endregion
		}

		[Test]
		public void UnityRepositoryTestGetAllCommonUnitiesException()
		{
			#region Arrange
			dbContext.Setup(x => x.Unities).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _unityRepository.GetAllCommonUnities());
		}

		[Test]
		public void UnityRepositoryTestGetUnityById()
		{
			#region expected
			Unity expected = new Unity
			{
				Id = 1,
				IdSpecialty = 1,
				Name = "unidad1",
				Description = "unidad1"
			};
			#endregion

			#region Actual
			Unity? actual = _unityRepository.GetUnityById(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			CollectionAssert.AreEqual(expected.Users, actual.Users);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			Assert.That(actual.IdSpecialty, Is.EqualTo(expected.IdSpecialty));
			Assert.That(actual.IdSpecialtyNavigation, Is.EqualTo(expected.IdSpecialtyNavigation));
			#endregion
		}

		[Test]
		public void UnityRepositoryTestGetlSpecialtyByIdException()
		{
			#region Arrange
			dbContext.Setup(x => x.Unities).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _unityRepository.GetUnityById(It.IsAny<int>()));
		}

		[Test]
		public void UnityRepositoryTestGetlSpecialtyByName()
		{
			#region expected
			Unity expected = new Unity
			{
				Id = 1,
				IdSpecialty = 1,
				Name = "unidad1",
				Description = "unidad1"
			};
			#endregion

			#region Actual
			Unity? actual = _unityRepository.GetUnityByName("unidad1").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			CollectionAssert.AreEqual(expected.Users, actual.Users);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			Assert.That(actual.IdSpecialtyNavigation, Is.EqualTo(expected.IdSpecialtyNavigation));
			#endregion
		}

		[Test]
		public void UnityRepositoryTestGetlSpecialtyByNameException()
		{
			#region Arrange
			dbContext.Setup(x => x.Unities).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _unityRepository.GetUnityByName(It.IsAny<string>()));
		}


		[TestCaseSource(nameof(GetAddUnityCase))]
		[Test]
		public void UnityRepositoryTestAddUnity(Unity unity, bool expected, int expectedAddUnity, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int addUnity = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Unities.AddAsync(It.IsAny<Unity>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addUnity = callCount++);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_unityRepository.AddUnity(unity);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Unities.AddAsync(It.IsAny<Unity>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addUnity, Is.EqualTo(expectedAddUnity));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void UnityRepositoryTestAddUnityException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _unityRepository.AddUnity(It.IsAny<Unity>()));
		}

		[TestCaseSource(nameof(GetAddUnitiesCase))]
		[Test]
		public void UnityServiceTestAddUnities(List<Unity> unities, bool expected, int expectedAddUnity, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int addUnity = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Unities.AddRangeAsync(It.IsAny<List<Unity>>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addUnity = ++callCount);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_unityRepository.AddUnities(unities);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Unities.AddRangeAsync(It.IsAny<List<Unity>>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addUnity, Is.EqualTo(expectedAddUnity));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void UnityServiceTestAddUnitiesException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _unityRepository.AddUnities(It.IsAny<List<Unity>>()));
		}

		[TestCaseSource(nameof(GetUpdateUnityCase))]
		[Test]
		public void UnityRepositoryTestUpdateUnity(Unity unity, bool expected, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => saveChanges = callCount++);
			dbContext.Setup<DbSet<Unity>>(x => x.Unities).ReturnsDbSet(GetFakeUnities());

			#endregion
			_unityRepository.UpdateUnity(unity);

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
		public void UnityRepositoryTestUpdateUnityException()
		{
			#region Arrange
			Unity unity = new Unity()
			{
				Id = 1,
				IdSpecialty = 1,
				Name = "unidad5",
				Description = "unidad5"
			};

			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _unityRepository.UpdateUnity(unity));
		}

		[TestCaseSource(nameof(GetDeleteUnityCase))]
		[Test]
		public void UnityRepositoryTestDeleteUnity(int id, bool expected, int expectedDeleteUnity, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int deleteUnity = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Unities.Remove(It.IsAny<Unity>())).Callback(() => deleteUnity = callCount++);
			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => saveChanges = callCount++);

			#endregion
			_unityRepository.DeleteUnity(id);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Unities.Remove(It.IsAny<Unity>()), Times.Once());
				dbContext.Verify(x => x.SaveChangesAsync(default), Times.Once());
			}

			#region Assert
			Assert.That(deleteUnity, Is.EqualTo(expectedDeleteUnity));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void UnityRepositoryTestDeleteUnityException()
		{
			#region Arrange
			int callCount = 0;
			int deleteUnity = 0;
			dbContext.Setup(x => x.Unities.Remove(It.IsAny<Unity>())).Callback(() => deleteUnity = callCount++);
			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _unityRepository.DeleteUnity(1));
		}

		private static object[] GetDeleteUnityCase =
		{
			new object[] { 1, true, 0, 1},
			new object[] { 10, false, 0, 0 }
		};

		private static object[] GetAddUnityCase =
		{
			new object[] { new Unity() {
					IdSpecialty = 1,
					Name = "unidad9",
					Description = "unidad9",
			}, true, 0, 1},
			new object[] { new Unity() {
					IdSpecialty = 1,
					Name = "unidad1",
					Description = "unidad1"
			}, false, 0, 0 }
		};

		private static object[] GetAddUnitiesCase =
		{
			new object[] { new List<Unity>() {
				new Unity() {
					IdSpecialty = 1,
					Name = "unidad9",
					Description = "unidad9",
				},
				new Unity() {
					IdSpecialty = 1,
					Name = "unidad10",
					Description = "unidad10",
				}
			}, true, 1, 1},
			new object[] { new List<Unity>() {
				new Unity() {
					IdSpecialty = 1,
					Name = "unidad1",
					Description = "unidad1"
				} 
			}, false, 0, 0 },
			new object[] { new List<Unity>() {
				new Unity() {
					IdSpecialty = 1,
					Name = "unidad9",
					Description = "unidad9",
				},
				new Unity() {
					IdSpecialty = 1,
					Name = "unidad1",
					Description = "unidad1",
				}
			}, true, 1, 1}
		};

		private static object[] GetUpdateUnityCase =
		{
			new object[] { new Unity() {
					Id = 1,
					IdSpecialty = 1,
					Name = "unidad1",
					Description = "unidad1"
			}, true, 0},
			new object[] { new Unity() {
					Id = 15,
					IdSpecialty = 1,
					Name = "unidad15",
					Description = "unidad15"
			}, false, 0}
		};

		private List<Unity> GetFakeUnities()
		{
			return new List<Unity>()
			{
				new Unity()
				{
					Id = 1,
					Description = "unidad1",
					Name = "unidad1",
					IdSpecialty = 1
				},
				new Unity()
				{
					Id = 2,
					Description = "unidad2",
					Name = "unidad2",
					IdSpecialty = 1
				},
				new Unity()
				{
					Id = 3,
					Description = "unidad3",
					Name = "unidad3",
					IdSpecialty = 2
				},
				new Unity()
				{
					Id = 4,
					Description = "unidad4",
					Name = "unidad4",
					IdSpecialty = 2
				},
				new Unity()
				{
					Id = 5,
					Description = "unidad5",
					Name = "unidad5",
					IdSpecialty = 3
				},
				new Unity()
				{
					Id = 6,
					Description = "unidad6",
					Name = "unidad6",
					IdSpecialty = 3
				}, 
				new Unity()
				{
					Id = 7,
					Description = "unidad7",
					Name = "unidad7",
					IdSpecialty = 4
				},
				new Unity()
				{
					Id = 8,
					Description = "unidad8",
					Name = "unidad8",
					IdSpecialty = 4
				},
				new Unity()
				{
					Id = 9,
					Description = "rotatorio",
					Name = "rotatorio"
				}
			};
		}
	}
}
