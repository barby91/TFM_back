using Moq;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Test.Services
{
	public class LevelServiceTest
	{
		private Mock<ILevelRepository<Level>> _levelRepository;
		private ILevelService _levelService;

		[SetUp]
		public void Setup()
		{
			_levelRepository = new Mock<ILevelRepository<Level>>();
			_levelService = new LevelService(_levelRepository.Object);
		}

		[Test]
		public void LevelServiceTestGetAllLevels()
		{
			#region Expected
			List<LevelModel> expected = new List<LevelModel>()
			{
				new LevelModel
				{
					Id = 1,
					Name = "level1"
				},
				new LevelModel
				{
					Id = 2,
					Name = "level2"
				}
			};
			#endregion

			#region Arrange
			_levelRepository.Setup(ur => ur.GetAllLevels()).ReturnsAsync(new List<Level>()
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
																		 });
			#endregion

			#region Actual
			List<LevelModel> actual = _levelService.GetAllLevels().Result;
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
		public void LevelServiceTestGetAllLevelsException()
		{
			#region Arrange
			_levelRepository.Setup(x => x.GetAllLevels()).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _levelService.GetAllLevels());
		}

		[Test]
		public void SpecialtyRepositoryTestGetlevelByName()
		{
			#region expected
			LevelModel expected = new LevelModel
			{
				Id = 1,
				Name = "level1"
			};
			#endregion

			#region Arrange
			_levelRepository.Setup(ur => ur.GetLevelByName("level1")).ReturnsAsync(new Level
																					{
																						Id = 1,
																						Description = "level1",
																						Name = "level1"
																					});
			#endregion

			#region Actual
			LevelModel? actual = _levelService.GetLevelByName("level1").Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual); Assert.IsNotNull(actual);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Name, Is.EqualTo(expected.Name));
			#endregion
		}

		[Test]
		public void LevelRepositoryTestGetLevelByNameException()
		{
			#region Arrange
			_levelRepository.Setup(x => x.GetLevelByName(It.IsAny<string>())).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _levelService.GetLevelByName("level1"));
		}
	}
}
