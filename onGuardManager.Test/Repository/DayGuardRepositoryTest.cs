using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Models.Entities;
using Assert = NUnit.Framework.Assert;

namespace onGuardManager.Test.Repository
{
	public class DayGuardRepositoryTest
	{
		private IDayGuardRepository<DayGuard> _dayGuardRepository;
		private Mock<OnGuardManagerContext> dbContext;

		[SetUp]
		public void Setup()
		{
			dbContext = new Mock<OnGuardManagerContext>();
			dbContext.Setup<DbSet<DayGuard>>(x => x.DayGuards)
				.ReturnsDbSet(GetFakedayGuards());
			_dayGuardRepository = new DayGuardRepository(dbContext.Object);
		}

		[TestCaseSource(nameof(GetAddGuardsCase))]
		[Test]
		public void DayGuardRepositoryTestSaveGuard(DayGuard dayGuard, bool expected, int expectedAddGuards, int expectedSaveChanges)
		{

			#region Arrange		
			int callCount = 0;
			int addGuards = 0;
			int saveChanges = 0;

			dbContext.Setup(x => x.DayGuards.AddAsync(It.IsAny<DayGuard>(), It.IsAny<System.Threading.CancellationToken>())).Callback(() => addGuards = ++callCount);
			dbContext.Setup(x => x.SaveChanges()).Callback(() => saveChanges = callCount++);

			#endregion
			_dayGuardRepository.SaveGuard(dayGuard);

			if (expected)
			{
				// Check that each method was only called once.
				dbContext.Verify(x => x.DayGuards.AddAsync(It.IsAny<DayGuard>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once());
				dbContext.Verify(x => x.SaveChanges(), Times.Once());
			}

			#region Assert
			Assert.That(addGuards, Is.EqualTo(expectedAddGuards));
			Assert.That(saveChanges, Is.EqualTo(expectedSaveChanges));
			#endregion
		}
		
		[Test]
		public void DayGuardRepositoryTestSaveGuardException()
		{
			#region Arrange	
			dbContext.Setup(x => x.SaveChanges()).Callback(() => throw new Exception());
			#endregion
			Assert.ThrowsAsync<Exception>(async() => await _dayGuardRepository.SaveGuard(new DayGuard
																										{
																											Day = new DateOnly(2024, 01, 02),
																											assignedUsers = new List<User>()
																																{
																																	new User()
																																	{
																																		Id = 1,
																																		Name = "usuario1",
																																		IdCenter = 1,
																																		IdLevel = 1,
																																		IdSpecialty = 1,
																																		Surname = "usuario"
																																	}
																																}
																										}));
		}
		
		[TestCaseSource(nameof(GetDeleteGuardsCase))]
		[Test]
		//public void DayGuardRepositoryTestDeletePreviousGuard(DateOnly initDate, DateOnly finalDate, bool expected)
		//{
		//	#region Actual
		//	bool actual = _dayGuardRepository.DeletePreviousGuard(initDate, finalDate).Result;
		//	#endregion

		//	#region Assert
		//	Assert.That(actual, Is.EqualTo(expected));
		//	#endregion
		//}
		public void DayGuardRepositoryTestDeletePreviousGuard(int month, bool expected)
		{
			#region Actual
			bool actual = _dayGuardRepository.DeletePreviousGuard(month).Result;
			#endregion

			#region Assert
			Assert.That(actual, Is.EqualTo(expected));
			#endregion
		}

		[Test]
		public void DayGuardRepositoryTestGetlDeletePreviousGuardException()
		{
			#region Arrange
			dbContext.Setup(x => x.SaveChangesAsync(default)).Callback(() => throw new Exception());
			#endregion

			//Assert.ThrowsAsync<Exception>(async() => await _dayGuardRepository.DeletePreviousGuard(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()));
			Assert.ThrowsAsync<Exception>(async () => await _dayGuardRepository.DeletePreviousGuard(It.IsAny<int>()));
		}
		
		[TestCaseSource(nameof(GetGuardsCase))]
		[Test]
		public void DayGuardRepositoryTestGetGuards(List<DayGuard> expected, int center, int year, int month)
		{
			#region Actual
			List<DayGuard> actual = _dayGuardRepository.GetGuards(center, year, month).Result;
			#endregion

			#region Assert
			Assert.IsNotNull(actual);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < actual.Count; i++)
			{
				Assert.That(actual[i].assignedUsers.Count, Is.EqualTo(expected[i].assignedUsers.Count));
				for (int j = 0; j < actual[i].assignedUsers.Count; j++)
				{
					Assert.That(actual[i].assignedUsers.ToList()[j].Id, Is.EqualTo(expected[i].assignedUsers.ToList()[j].Id));
					Assert.That(actual[i].assignedUsers.ToList()[j].Name, Is.EqualTo(expected[i].assignedUsers.ToList()[j].Name));
					Assert.That(actual[i].assignedUsers.ToList()[j].Surname, Is.EqualTo(expected[i].assignedUsers.ToList()[j].Surname));
					Assert.That(actual[i].assignedUsers.ToList()[j].IdCenter, Is.EqualTo(expected[i].assignedUsers.ToList()[j].IdCenter));
					Assert.That(actual[i].assignedUsers.ToList()[j].IdSpecialty, Is.EqualTo(expected[i].assignedUsers.ToList()[j].IdSpecialty));
					Assert.That(actual[i].assignedUsers.ToList()[j].IdLevel, Is.EqualTo(expected[i].assignedUsers.ToList()[j].IdLevel));
				}
				Assert.That(actual[i].Id, Is.EqualTo(expected[i].Id));
				Assert.That(actual[i].Day, Is.EqualTo(expected[i].Day));
			}
			#endregion
		}

		[Test]
		public void DayGuardRepositoryTestGetGuardsException()
		{
			#region Arrange
			dbContext.Setup(x => x.DayGuards).Callback(() => throw new Exception());
			#endregion

			Assert.ThrowsAsync<Exception>(async() => await _dayGuardRepository.GetGuards(1, 2024, 1));

		}

		private List<DayGuard> GetFakedayGuards()
		{
			return new List<DayGuard>()
			{
				new DayGuard
				{
					Id = 1,
					Day = new DateOnly(2024, 01, 01),
					assignedUsers = new List<User>()
					{
						new User()
						{
							Id = 1,
							Name = "usuario1",
							IdCenter = 1,
							IdLevel = 1,
							IdSpecialty = 1,
							Surname = "usuario"
						}
					}
				},
				new DayGuard
				{
					Id = 1,
					Day = new DateOnly(2024, 02, 01),
					assignedUsers = new List<User>()
					{
						new User()
						{
							Id = 1,
							Name = "usuario1",
							IdCenter = 1,
							IdLevel = 1,
							IdSpecialty = 1,
							Surname = "usuario"
						}
					}
				}
			};
		}

		private static object[] GetAddGuardsCase =
		{
			new object[] { new DayGuard
							{
								Day = new DateOnly(2024, 01, 01),
								assignedUsers = new List<User>()
								{
									new User()
									{
										Id = 1,
										Name = "usuario1",
										IdCenter = 1,
										IdLevel = 1,
										IdSpecialty = 1,
										Surname = "usuario"
									}
								}
							}, true, 1, 1},
			new object[] { new DayGuard
							{
								Day = new DateOnly(2024, 03, 01),
								assignedUsers = new List<User>()
								{
									new User()
									{
										Id = 1,
										Name = "usuario1",
										IdCenter = 1,
										IdLevel = 1,
										IdSpecialty = 1,
										Surname = "usuario"
									}
								}
							}, true, 1, 1}
		};

		private static object[] GetDeleteGuardsCase =
		{
			new object[] {1, true},
			new object[] {12, true},
		};

		private static object[] GetGuardsCase =
		{
			new object[] { new List<DayGuard>()
							{
								new DayGuard
								{
									Id = 1,
									Day = new DateOnly(2024, 01, 01),
									assignedUsers = new List<User>()
									{
										new User()
										{
											Id = 1,
											Name = "usuario1",
											IdCenter = 1,
											IdLevel = 1,
											IdSpecialty = 1,
											Surname = "usuario"
										}
									}
								}
							}, 1, 2024, 1},
			new object[] { new List<DayGuard>()
							{
								new DayGuard
								{
									Id = 1,
									Day = new DateOnly(2024, 01, 01),
									assignedUsers = new List<User>()
									{
										new User()
										{
											Id = 1,
											Name = "usuario1",
											IdCenter = 1,
											IdLevel = 1,
											IdSpecialty = 1,
											Surname = "usuario"
										}
									}
								},
								new DayGuard
								{
									Id = 1,
									Day = new DateOnly(2024, 02, 01),
									assignedUsers = new List<User>()
									{
										new User()
										{
											Id = 1,
											Name = "usuario1",
											IdCenter = 1,
											IdLevel = 1,
											IdSpecialty = 1,
											Surname = "usuario"
										}
									}
								}
							}, 1, 2024, 0}
		};
	}
}
