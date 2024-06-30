using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Repository
{
	public class RolRepositoryTest
	{
		private IRolRepository<Rol> _rolRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<Rol>>(x => x.Rols)
				.ReturnsDbSet(GetFakeRols());
			_rolRepository = new RolRepository(dbContext.Object);
		}

		[Test]
		public void RolRepositoryTestGetAllRols()
		{
			#region expected
			List<Rol> expected = new List<Rol>()
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
			};
			#endregion

			#region Actual
			List<Rol> actual = _rolRepository.GetAllRols().Result;
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
			}
			#endregion
		}

		[Test]
		public void RolRepositoryTestGetAllRolsException()
		{
			#region Arrange
			dbContext.Setup(x => x.Rols).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _rolRepository.GetAllRols());
		}

		[Test]
		public void RolRepositoryTestGetRolByName()
		{
			#region expected
			Rol expected = new Rol()
								{
									Id = 4,
									Name = "rol4",
									Description = "rol4"
								};
			#endregion

			#region Actual
			Rol? actual = _rolRepository.GetRolByName("rol4").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			#endregion
		}

		[Test]
		public void RolRepositoryTestGetRolByNameException()
		{
			#region Arrange
			dbContext.Setup(x => x.Rols).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _rolRepository.GetRolByName(It.IsAny<string>()));
		}

		private List<Rol> GetFakeRols()
		{
			return new List<Rol>()
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
			};
		}
	}
}
