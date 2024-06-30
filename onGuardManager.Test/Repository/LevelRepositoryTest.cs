using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Repository
{
	public class LevelRepositoryTest
	{
		private ILevelRepository<Level> _levelRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<Level>>(x => x.Levels)
				.ReturnsDbSet(GetFakeLevels());
			_levelRepository = new LevelRepository(dbContext.Object);
		}

		[Test]
		public void LevelRepositoryTestGetAllLevels()
		{
			#region Expected
			List<Level> expected = new List<Level>()
			{
				new Level
				{
					Id = 1,
					Description = "level1",
					Name = "level1"
				},
				new Level
				{
					Id = 2,
					Description = "level2",
					Name = "level2"
				}
			};
			#endregion

			#region Actual
			List<Level> actual = _levelRepository.GetAllLevels().Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
				Assert.That(actual[i].Description, Is.EqualTo(expected[i].Description));
				CollectionAssert.AreEqual(actual[i].Users, expected[i].Users);
			}
			#endregion
		}

		[Test]
		public void LevelRepositoryTestGetAllLevelsException()
		{
			#region Arrange
			dbContext.Setup(x => x.Levels).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _levelRepository.GetAllLevels());
		}

		[Test]
		public void LevelRepositoryTestGetLevelByName()
		{
			#region expected
			Level expected = new Level
			{
				Id = 1,
				Description = "level1",
				Name = "level1"
			};
			#endregion

			#region Actual
			Level? actual = _levelRepository.GetLevelByName("level1").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			CollectionAssert.AreEqual(expected.Users, actual.Users);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			Assert.That(actual.Description, Is.EqualTo(expected.Description));
			#endregion
		}

		[Test]
		public void LevelRepositoryTestGetlSpecialtyByNameException()
		{
			#region Arrange
			dbContext.Setup(x => x.Levels).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _levelRepository.GetLevelByName(It.IsAny<string>()));
		}

		private List<Level> GetFakeLevels()
		{
			return new List<Level>()
			{
				new Level
				{
					Id = 1,
					Description = "level1",
					Name = "level1"
				},
				new Level
				{
					Id = 2,
					Description = "level2",
					Name = "level2"
				}
			};
		}
	}
}
