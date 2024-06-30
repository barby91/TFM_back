using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography.Xml;

namespace onGuardManager.Test.Services
{
	public class SpecialtyServiceTest
	{
		private Mock<ISpecialtyRepository<Specialty>> _specialtyRepository;
		private ISpecialtyService _serviceSpecialty;

		[SetUp]
		public void Setup()
		{
			_specialtyRepository = new Mock<ISpecialtyRepository<Specialty>>();
			_serviceSpecialty = new SpecialtyService(_specialtyRepository.Object);
		}

		[Test]
		public void SpecialtyServiceTestGetAllSpecialtiesWithAllUnitiesByCenterCenter()
		{
			#region expected
			List<SpecialtyModel> expected = new List<SpecialtyModel>()
			{
				new SpecialtyModel
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1"
				},
				new SpecialtyModel
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad4",
					Description = "especialidad4"
				}
			};
			#endregion

			#region Arrange
			_specialtyRepository.Setup(ur => ur.GetAllSpecialtiesWithAllUnitiesByCenter(1)).ReturnsAsync(new List<Specialty>()
																						   {
																						   		new Specialty
																						   		{
																						   			Id = 1,
																						   			IdCenter = 1,
																						   			Name = "especialidad1",
																						   			Description = "especialidad1",
																									MaxGuards = 6
																						   		},
																						   		new Specialty
																						   		{
																						   			Id = 1,
																						   			IdCenter = 1,
																						   			Name = "especialidad4",
																						   			Description = "especialidad4",
																									MaxGuards = 6
																						   		}
																						   });
			#endregion

			#region Actual
			List<SpecialtyModel> actual = _serviceSpecialty.GetAllSpecialtiesByCenter(1).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].Description, Is.EqualTo(expected[i].Description));
				Assert.That(actual[i].IdCenter, Is.EqualTo(expected[i].IdCenter));
			}
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestGetAllSpecialtiesWithAllUnitiesByCenterException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _serviceSpecialty.GetAllSpecialtiesByCenter(It.IsAny<int>()));
		}

		[TestCaseSource(nameof(GetSpecialtiesByIdCase))]
		[Test]
		public void SpecialtyServiceTestGetlSpecialtyById(SpecialtyModel expected, Specialty resultObject, bool isNull)
		{
			#region expected
			
			#endregion

			#region Arrange
			_specialtyRepository.Setup(ur => ur.GetSpecialtyById(1)).ReturnsAsync(resultObject);
			#endregion

			#region Actual
			SpecialtyModel? actual = _serviceSpecialty.GetSpecialtyById(1).Result;
			#endregion

			#region Assert
			if(!isNull)
			{ 
				Assert.IsNotNull(actual);
				Assert.That(actual.Id, Is.EqualTo(expected.Id));
				Assert.That(actual.Name, Is.EqualTo(expected.Name));
				Assert.That(actual.Description, Is.EqualTo(expected.Description));
				Assert.That(actual.IdCenter, Is.EqualTo(expected.IdCenter));
				Assert.That(actual.Unities.Count, Is.EqualTo(expected.Unities.Count));
				for(int i = 0; i < actual.Unities.Count; i++)
				{
					Assert.That(actual.Unities[i].Id, Is.EqualTo(expected.Unities[i].Id));
					Assert.That(actual.Unities[i].Name, Is.EqualTo(expected.Unities[i].Name));
					Assert.That(actual.Unities[i].Description, Is.EqualTo(expected.Unities[i].Description));
				}
			}
			else
			{
				Assert.IsNull(actual);
			}
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestGetlSpecialtyByIdException()
		{
			#region Arrange
			_specialtyRepository.Setup(x => x.GetSpecialtyById(It.IsAny<int>())).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceSpecialty.GetSpecialtyById(1));
		}

		[Test]
		public void SpecialtyServiceTestGetlSpecialtyByName()
		{
			#region expected
			SpecialtyModel expected = new SpecialtyModel
			{
				Id = 1,
				IdCenter = 1,
				Name = "especialidad1",
				Description = "especialidad1",
				MaxGuards = 6
			};
			#endregion

			#region Arrange
			_specialtyRepository.Setup(ur => ur.GetSpecialtyByName("especialidad1")).ReturnsAsync(new Specialty
																								  {
																								  		Id = 1,
																								  		IdCenter = 1,
																								  		Name = "especialidad1",
																								  		Description = "especialidad1",
																										MaxGuards = 6
			});
			#endregion

			#region Actual
			SpecialtyModel? actual = _serviceSpecialty.GetSpecialtyByName("especialidad1").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			Assert.That(actual.IdCenter, Is.EqualTo(expected.IdCenter));
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestGetlSpecialtyByNameException()
		{
			#region Arrange
			_specialtyRepository.Setup(x => x.GetSpecialtyByName(It.IsAny<string>())).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceSpecialty.GetSpecialtyByName("especialidad1"));
		}

		[TestCaseSource(nameof(GetAddSpecialtyCase))]
		[Test]
		public void SpecialtyServiceTestAddSpecialty(Specialty specialty, bool expected)
		{
			#region Arrange
			_specialtyRepository.Setup(ur => ur.AddSpecialty(specialty)).ReturnsAsync(expected);
			#endregion

			bool actual = _serviceSpecialty.AddSpecialty(specialty).Result;

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestAddSpecialtyException()
		{
			#region Arrange
			Specialty specialty = new Specialty()
			{
				IdCenter = 1,
				Name = "especialidad5",
				Description = "especialidad5",
				MaxGuards = 6
			};

			_specialtyRepository.Setup(ur => ur.AddSpecialty(specialty)).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceSpecialty.AddSpecialty(specialty));
		}

		[TestCaseSource(nameof(GetAddSpecialtiesCase))]
		[Test]
		public void SpecialtyServiceTestAddSpecialties(List<Specialty> specialties, List<SpecialtyModel> specialtiesModel, bool expected)
		{
			#region Arrange
			List<Specialty> specialtiesMap = new List<Specialty>();
			foreach (SpecialtyModel specialty in specialtiesModel)
			{
				specialtiesMap.Add(specialty.Map());
			}

			_specialtyRepository.Setup(ur => ur.AddSpecialties(It.IsAny<List<Specialty>>())).ReturnsAsync(expected);
			#endregion

			bool actual = _serviceSpecialty.AddSpecialties(specialtiesModel).Result;

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestAddSpecialtiesException()
		{
			Assert.ThrowsAsync<NullReferenceException>(async() => await _serviceSpecialty.AddSpecialties(It.IsAny<List<SpecialtyModel>>()));
		}

		[TestCaseSource(nameof(GetUpdateSpecialtyCase))]
		[Test]
		public void SpecialtyServiceTestUpdateSpecialty(Specialty specialty, bool expected)
		{
			#region Arrange		
			_specialtyRepository.Setup(ur => ur.UpdateSpecialty(specialty)).ReturnsAsync(expected);
			#endregion

			bool actual = _serviceSpecialty.UpdateSpecialty(specialty).Result;

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestUpdateSpecialtyException()
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

			_specialtyRepository.Setup(x => x.UpdateSpecialty(It.IsAny<Specialty>())).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceSpecialty.UpdateSpecialty(specialty));
		}

		[TestCaseSource(nameof(GetDeleteSpecialtyCase))]
		[Test]
		public void SpecialtyServiceTestDeleteSpecialty(int id, bool expected)
		{
			#region Arrange
			_specialtyRepository.Setup(ur => ur.DeleteSpecialty(id)).ReturnsAsync(expected);
			#endregion

			#region Actual
			bool actual = _serviceSpecialty.DeleteSpecialty(id).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void SpecialtyServiceTestDeleteSpecialtyException()
		{
			#region Arrange
			_specialtyRepository.Setup(x => x.DeleteSpecialty(1)).Throws(() => new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceSpecialty.DeleteSpecialty(1));
		}

		private static object[] GetDeleteSpecialtyCase =
		{
			new object[] { 1, true},
			new object[] { 4, false}
		};

		private static object[] GetSpecialtiesByIdCase =
		{
			new object[]
			{
				new SpecialtyModel
				{
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					Unities = new List<UnityModel>()
					{
						new UnityModel()
						{
							Description = "unidad",
							Name = "unidad",
							Id = 1
						}
					}
				},
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
							Description = "unidad",
							Name = "unidad",
							Id = 1
						}
					}
				},
				false
			},
			new object?[] {
				null,
				new Specialty
				{
					Id = 0,
				},
				true
			},
			new object?[] {
				null,
				null,
				true
			}
		};

		private static object[] GetAddSpecialtyCase =
		{
			new object[] { new Specialty() {
					IdCenter = 1,
					Name = "especialidad5",
					Description = "especialidad5",
					MaxGuards = 6
			}, true},
			new object[] { new Specialty() {
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6
			}, false}
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
			}, new List<SpecialtyModel>(){
				new SpecialtyModel() {
					IdCenter = 1,
					Name = "especialidad5",
					Description = "especialidad5",
					MaxGuards = 6,
					Unities = new List<UnityModel>()
					{
						new UnityModel()
						{
							Description = "Unidad",
							Name = "Unidad",
							Id =1
						}
					}
				},
				new SpecialtyModel() {
					IdCenter = 1,
					Name = "especialidad6",
					Description = "especialidad6",
					MaxGuards = 6
				}
			}, true},
			new object[] {new List<Specialty>(){
				new Specialty() {
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6
				}
			}, new List<SpecialtyModel>(){
				new SpecialtyModel() {
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1",
					MaxGuards = 6
				}
			}, false}
		};

		private static object[] GetUpdateSpecialtyCase =
		{
			new object[] { new Specialty() {
					Id = 1,
					IdCenter = 1,
					Name = "especialidad1",
					Description = "especialidad1"
			}, true},
			new object[] { new Specialty() {
					Id = 8,
					IdCenter = 1,
					Name = "especialidad8",
					Description = "especialidad8"
			}, false}
		};
	}
}