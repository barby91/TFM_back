using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Services
{
	public class RolServiceTest
	{
		private Mock<IRolRepository<Rol>> _rolRepository;
		private IRolService _serviceRol;

		[SetUp]
		public void Setup()
		{
			_rolRepository = new Mock<IRolRepository<Rol>>();
			_serviceRol = new RolService(_rolRepository.Object);
		}

		[Test]
		public void RolServiceTestGetAllRols()
		{
			#region expected
			List<RolModel> expected = new List<RolModel>()
			{
				new RolModel
				{
					Id = 1,
					Name = "rol1"
				},
				new RolModel
				{
					Id = 2,
					Name = "rol2"
				},
				new RolModel
				{
					Id = 3,
					Name = "rol3"
				},
				new RolModel
				{
					Id = 4,
					Name = "rol4"
				}
			};

			#endregion

			#region Arrange
			_rolRepository.Setup(ur => ur.GetAllRols()).ReturnsAsync(new List<Rol>()
																	 {
																		new Rol
																		{
																			Id = 1,
																			Name = "rol1",
																			Description = "rol1"
																		},
																		new Rol
																		{
																			Id = 2,
																			Name = "rol2",
																			Description = "rol2"
																		},
																		new Rol
																		{
																			Id = 3,
																			Name = "rol3",
																			Description = "rol3"
																		},
																		new Rol
																		{
																			Id = 4,
																			Name = "rol4",
																			Description = "rol4"
																		}
																	 });
			#endregion

			#region Actual
			List<RolModel> actual = _serviceRol.GetAllRols().Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
			}
			#endregion
		}

		[Test]
		public void RolServiceTestGetAllRolsException()
		{
			#region Arrange
			_rolRepository.Setup(x => x.GetAllRols()).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceRol.GetAllRols());
		}

		[Test]
		public void RolServiceTestGetRolByName()
		{
			#region expected
			RolModel expected = new RolModel
			{
				Id = 1,
				Name = "rol1"
			};
			#endregion

			#region Arrange
			_rolRepository.Setup(ur => ur.GetRolByName("rol1")).ReturnsAsync(new Rol
																			 {
																			 	Id = 1,
																			 	Name = "rol1",
																			 	Description = "rol1"
																			 });
			#endregion

			#region Actual
			RolModel? actual = _serviceRol.GetRolByName("rol1").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			#endregion
		}

		[Test]
		public void RolServiceTestGetRolByNameException()
		{
			#region Arrange
			_rolRepository.Setup(x => x.GetRolByName(It.IsAny<string>())).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _serviceRol.GetRolByName("rol1"));
		}
	}
}
