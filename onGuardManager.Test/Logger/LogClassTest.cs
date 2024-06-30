using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Logger;
using onGuardManager.Models;

namespace onGuardManager.Test.Repository
{
	public class LogClassTest
	{
		[SetUp]
		public void Setup()
		{
			
		}

		[TestCaseSource(nameof(GetLogData))]
		[Test]
		public void LogClassTestWriteLog(ErrorWrite errorLog, string message)
		{
			Assert.DoesNotThrow(() => LogClass.WriteLog(errorLog, message));
		}

		[Test]
		public void LogClassTestFlushNLog()
		{
			Assert.DoesNotThrow(() => LogClass.FlushNLog());
		}


		private static object[] GetLogData =
		{
			new object[] { ErrorWrite.Info, "Mensaje de información"},
			new object[] { ErrorWrite.Debug, "Mensaje de debug"},
			new object[] { ErrorWrite.Error, "Mensaje de error"},
			new object[] { ErrorWrite.Trace, "Mensaje de traza"},
			new object[] { ErrorWrite.Nothing, "Mensaje default de debug"}
		};
	}
}
