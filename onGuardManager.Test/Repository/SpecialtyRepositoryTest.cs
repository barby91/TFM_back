using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Repository
{
	public class SpecialtyRepositoryTest
	{
		private ISpecialtyRepository<Specialty> _specialtyRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<Specialty>>(x => x.Specialties)
				.ReturnsDbSet(GetFakeSpecialties());
			_specialtyRepository = new SpecialtyRepository(dbContext.Object);
		}

		[Test]
		public void SpecialtyRepositoryTestGetAllSpecialtiesWithAllUnitiesByCenter()
		{
			#region arrange
			dbContext.Setup<DbSet<Unity>>(x => x.Unities)
				.ReturnsDbSet(GetFakeUnities());
			#endregion

			#region expected
			List<Specialty> expected = new List<Specialty>()
			{
				new Specialty
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 1,
							Description = "unidad1",
							Name = "unidad1"
						},
						new Unity()
						{
							Id = 2,
							Description = "unidad2",
							Name = "unidad2"
						},
						new Unity()
						{
							Id = 9,
							Description = "rotatorio",
							Name = "rotatorio"
						}
					}
				},
				new Specialty
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad4",
					Description = "especialidad4",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 7,
							Description = "unidad7",
							Name = "unidad7"
						},
						new Unity()
						{
							Id = 8,
							Description = "unidad8",
							Name = "unidad8"
						},
						new Unity()
						{
							Id = 9,
							Description = "rotatorio",
							Name = "rotatorio"
						}
					}
				}
			};
			#endregion

			#region Actual
			List<Specialty> actual = _specialtyRepository.GetAllSpecialtiesWithAllUnitiesByCenter(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				CollectionAssert.AreEqual(expected[i].Users, actual[i].Users);
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].MaxGuards, Is.EqualTo(expected[i].MaxGuards));
				Assert.That(actual[i].Description, Is.EqualTo(expected[i].Description));
				Assert.That(actual[i].IdCenter, Is.EqualTo(expected[i].IdCenter));
				Assert.That(actual[i].Unities.Count, Is.EqualTo(expected[i].Unities.Count));
				for (int j = 0; j < actual[i].Unities.Count; j++)
				{
					Assert.That(actual[i].Unities.ToList()[j].Id, Is.EqualTo(expected[i].Unities.ToList()[j].Id));
					Assert.That(actual[i].Unities.ToList()[j].Name, Is.EqualTo(expected[i].Unities.ToList()[j].Name));
					Assert.That(actual[i].Unities.ToList()[j].Description, Is.EqualTo(expected[i].Unities.ToList()[j].Description));
					Assert.That(actual[i].Unities.ToList()[j].IdSpecialty, Is.EqualTo(expected[i].Unities.ToList()[j].IdSpecialty));
				}
			}
			#endregion
		}

		[Test]
		public void SpecialtyRepositoryTestGetAllSpecialtiesWithAllUnitiesByCenterException()
		{
			#region Arrange
			dbContext.Setup(x => x.Specialties).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _specialtyRepository.GetAllSpecialtiesWithAllUnitiesByCenter(It.IsAny<int>()));
		}

		[Test]
		public void SpecialtyRepositoryTestGetAllSpecialtiesWithoutCommonUnitiesByCenter()
		{
			#region arrange
			dbContext.Setup<DbSet<Unity>>(x => x.Unities)
				.ReturnsDbSet(GetFakeUnities());
			#endregion

			#region expected
			List<Specialty> expected = new List<Specialty>()
			{
				new Specialty
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 1,
							Description = "unidad1",
							Name = "unidad1"
						},
						new Unity()
						{
							Id = 2,
							Description = "unidad2",
							Name = "unidad2"
						}
					}
				},
				new Specialty
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad4",
					Description = "especialidad4",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 7,
							Description = "unidad7",
							Name = "unidad7"
						},
						new Unity()
						{
							Id = 8,
							Description = "unidad8",
							Name = "unidad8"
						}
					}
				}
			};
			#endregion

			#region Actual
			List<Specialty> actual = _specialtyRepository.GetAllSpecialtiesWithoutCommonUnitiesByCenter(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				CollectionAssert.AreEqual(expected[i].Users, actual[i].Users);
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].MaxGuards, Is.EqualTo(expected[i].MaxGuards));
				Assert.That(actual[i].Description, Is.EqualTo(expected[i].Description));
				Assert.That(actual[i].IdCenter, Is.EqualTo(expected[i].IdCenter));
				Assert.That(actual[i].Unities.Count, Is.EqualTo(expected[i].Unities.Count));
				for (int j = 0; j < actual[i].Unities.Count; j++)
				{
					Assert.That(actual[i].Unities.ToList()[j].Id, Is.EqualTo(expected[i].Unities.ToList()[j].Id));
					Assert.That(actual[i].Unities.ToList()[j].Name, Is.EqualTo(expected[i].Unities.ToList()[j].Name));
					Assert.That(actual[i].Unities.ToList()[j].Description, Is.EqualTo(expected[i].Unities.ToList()[j].Description));
					Assert.That(actual[i].Unities.ToList()[j].IdSpecialty, Is.EqualTo(expected[i].Unities.ToList()[j].IdSpecialty));
				}
			}
			#endregion
		}

		[Test]
		public void SpecialtyRepositoryTestGetAllSpecialtiesWithoutCommonUnitiesByCenterException()
		{
			#region Arrange
			dbContext.Setup(x => x.Specialties).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _specialtyRepository.GetAllSpecialtiesWithoutCommonUnitiesByCenter(It.IsAny<int>()));
		}

		[Test]
		public void SpecialtyRepositoryTestGetlSpecialtyById()
		{
			#region expected
			Specialty expected = new Specialty
			{
				Id = 1,
				IdCenter = 1,
				Name = "especialidad1",
				Description = "especialidad1",
				MaxGuards = 6
			};
			#endregion

			#region Actual
			Specialty? actual = _specialtyRepository.GetSpecialtyById(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			CollectionAssert.AreEqual(expected.Users, actual.Users);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.MaxGuards, Is.EqualTo(expected.MaxGuards));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			Assert.That(actual.IdCenter, Is.EqualTo(expected.IdCenter));
			#endregion
		}

		[Test]
		public void SpecialtyRepositoryTestGetlSpecialtyByIdException()
		{
			#region Arrange
			dbContext.Setup(x => x.Specialties).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _specialtyRepository.GetSpecialtyById(It.IsAny<int>()));
		}

		[Test]
		public void SpecialtyRepositoryTestGetlSpecialtyByName()
		{
			#region expected
			Specialty expected = new Specialty
			{
				Id = 1,
				IdCenter = 1,
				Name = "especialidad1",
				Description = "especialidad1",
				MaxGuards = 6
			};
			#endregion

			#region Actual
			Specialty? actual = _specialtyRepository.GetSpecialtyByName("especialidad1").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			CollectionAssert.AreEqual(expected.Users, actual.Users);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.MaxGuards, Is.EqualTo(expected.MaxGuards));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			Assert.That(actual.IdCenter, Is.EqualTo(expected.IdCenter));
			#endregion
		}

		[Test]
		public void SpecialtyRepositoryTestGetlSpecialtyByNameException()
		{
			#region Arrange
			dbContext.Setup(x => x.Specialties).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _specialtyRepository.GetSpecialtyByName(It.IsAny<string>()));
		}

		[TestCaseSource(nameof(GetAddSpecialtyCase))]
		[Test]
		public void SpecialtyRepositoryTestAddSpecialty(Specialty specialty, bool expected, int expectedAddSpecialty, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int addSpecialty = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Specialties.AddAsync(It.IsAny<Specialty>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addSpecialty = callCount++);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_specialtyRepository.AddSpecialty(specialty);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Specialties.AddAsync(It.IsAny<Specialty>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addSpecialty, Is.EqualTo(expectedAddSpecialty));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void SpecialtyRepositoryTestAddSpecialtyException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _specialtyRepository.AddSpecialty(It.IsAny<Specialty>()));
		}

		[TestCaseSource(nameof(GetAddSpecialtiesCase))]
		[Test]
		public void SpecialtyServiceTestAddSpecialties(List<Specialty> specialties, bool expected, int expectedAddSpecialty, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int addSpecialty = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Specialties.AddRangeAsync(It.IsAny<List<Specialty>>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addSpecialty = ++callCount);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_specialtyRepository.AddSpecialties(specialties);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Specialties.AddRangeAsync(It.IsAny<List<Specialty>>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addSpecialty, Is.EqualTo(expectedAddSpecialty));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestAddSpecialtiesException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _specialtyRepository.AddSpecialties(It.IsAny<List<Specialty>>()));
		}

		[TestCaseSource(nameof(GetUpdateSpecialtyCase))]
		[Test]
		public void SpecialtyRepositoryTestUpdateSpecialty(Specialty specialty, bool expected, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => saveChanges = callCount++);
			dbContext.Setup<DbSet<Specialty>>(x => x.Specialties).ReturnsDbSet(GetFakeSpecialties());
			dbContext.Setup<DbSet<Unity>>(x => x.Unities).ReturnsDbSet(GetFakeUnities());

			#endregion
			_specialtyRepository.UpdateSpecialty(specialty);

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
		public void SpecialtyRepositoryTestUpdateSpecialtyException()
		{
			#region Arrange
			Specialty specialty = new Specialty()
			{
				Id = 1,
				IdCenter = 1,
				Name = "especialidad5",
				Description = "especialidad5",
				MaxGuards = 6
			};

			dbContext.Setup(x => x.SaveChanges()).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<NullReferenceException>(async() => await _specialtyRepository.UpdateSpecialty(specialty));
		}

		[TestCaseSource(nameof(GetDeleteSpecialtyCase))]
		[Test]
		public void SpecialtyRepositoryTestDeleteSpecialty(int id, bool expected, int expectedDeleteSpecialty, int expectedSaveChanges)
		{
			#region Arrange		
			int callCount = 0;
			int deleteSpecialty = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.Specialties.Remove(It.IsAny<Specialty>())).Callback(() => deleteSpecialty = callCount++);
			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => saveChanges = callCount++);

			#endregion
			_specialtyRepository.DeleteSpecialty(id);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.Specialties.Remove(It.IsAny<Specialty>()), Times.Once());
				dbContext.Verify(x => x.SaveChangesAsync(default), Times.Once());
			}

			#region Assert
			Assert.That(deleteSpecialty, Is.EqualTo(expectedDeleteSpecialty));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}

		[Test]
		public void SpecialtyRepositoryTestDeleteSpecialtyException()
		{
			#region Arrange
			int callCount = 0;
			int deleteSpecialty = 0;
			dbContext.Setup(x => x.Specialties.Remove(It.IsAny<Specialty>())).Callback(() => deleteSpecialty = callCount++);
			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _specialtyRepository.DeleteSpecialty(1));
		}

		private static object[] GetDeleteSpecialtyCase =
		{
			new object[] { 1, true, 0, 1},
			new object[] { 4, false, 0, 0 }
		};

		private static object[] GetAddSpecialtyCase =
		{
			new object[] { new Specialty() {
					IdCenter = 1,
					Name = "especialidad5",
					Description = "especialidad5",
					MaxGuards = 6
			}, true, 0, 1},
			new object[] { new Specialty() {
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6
			}, false, 0, 0 }
		};

		private static object[] GetAddSpecialtiesCase =
		{
			new object[] { new List<Specialty>(){
				new Specialty() {
					IdCenter = 1,
					Name = "especialidad5",
					Description = "especialidad5",
					MaxGuards = 6
				},
				new Specialty() {
					IdCenter = 1,
					Name = "especialidad6",
					Description = "especialidad6",
					MaxGuards = 6
				}
			}, true, 1, 1},
			new object[] { new List<Specialty>(){
				new Specialty() {
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6
				}
			}, false, 0, 0 },
			new object[] { new List<Specialty>(){
				new Specialty() {
					IdCenter = 1,
					Name = "especialidad5",
					Description = "especialidad5",
					MaxGuards = 6
				},
				new Specialty() {
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6
				}
			}, true, 1, 1},
		};

		private static object[] GetUpdateSpecialtyCase =
		{
			new object[] { new Specialty() {
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6
			}, true, 0},
			new object[] { new Specialty() {
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 1,
							Description = "unidad1",
							Name = "unidad1"
						},
						new Unity()
						{
							Id = 2,
							Description = "unidad2",
							Name = "unidad2"
						}
					}
			}, true, 0},
			new object[] { new Specialty() {
					Id = 8,
					IdCenter = 1,
					Name = "especialidad8",
					Description = "especialidad8",
					MaxGuards = 6
			}, false, 0}
		};

		private List<Specialty> GetFakeSpecialties()
		{
			return new List<Specialty>()
			{
				new Specialty
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 1,
							Description = "unidad1",
							Name = "unidad1"
						},
						new Unity()
						{
							Id = 2,
							Description = "unidad2",
							Name = "unidad2"
						}
					}
				},
				new Specialty
				{
					Id = 1,
					IdCenter = 2,
					Name = "especialidad2",
					Description = "especialidad2",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 3,
							Description = "unidad3",
							Name = "unidad3"
						},
						new Unity()
						{
							Id = 4,
							Description = "unidad4",
							Name = "unidad4"
						}
					}
				},
				new Specialty
				{
					Id = 1,
					IdCenter = 2,
					Name = "especialidad3",
					Description = "especialidad3",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 5,
							Description = "unidad5",
							Name = "unidad5"
						},
						new Unity()
						{
							Id = 6,
							Description = "unidad6",
							Name = "unidad6"
						}
					}
				},
				new Specialty
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad4",
					Description = "especialidad4",
					MaxGuards = 6,
					Unities = new List<Unity>()
					{
						new Unity()
						{
							Id = 7,
							Description = "unidad7",
							Name = "unidad7"
						},
						new Unity()
						{
							Id = 8,
							Description = "unidad8",
							Name = "unidad8"
						}
					}
				}
			};
		}

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
