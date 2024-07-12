using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestFailedTests
{
	public class Cause
	{
		[Fact]
		public void GuardClause()
		{
			var failed = TestData.TestFailed();

			var ex = Record.Exception(() => failed.Cause = (FailureCause)2112);

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("Cause", argEx.ParamName);
			Assert.StartsWith($"Cause is not a valid value from {typeof(FailureCause).FullName}", argEx.Message);
		}
	}

	public class FromException
	{
		[Fact]
		public void NonAssertionException()
		{
			var ex = new DivideByZeroException();

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Exception, failed.Cause);
		}

		[Fact]
		public void BuiltInAssertionException()
		{
			var ex = EqualException.ForMismatchedValues(42, 2112);

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Assertion, failed.Cause);
		}

		[Fact]
		public void CustomAssertionException()
		{
			var ex = new MyAssertionException();

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Assertion, failed.Cause);
		}

		interface IAssertionException
		{ }

		class MyAssertionException : Exception, IAssertionException
		{ }

		[Fact]
		public void BuiltInTimeoutException()
		{
			var ex = TestTimeoutException.ForTimedOutTest(2112);

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Timeout, failed.Cause);
		}

		[Fact]
		public void CustomTimeoutException()
		{
			var ex = new MyTimeoutException();

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Timeout, failed.Cause);
		}

		interface ITestTimeoutException
		{ }

		class MyTimeoutException : Exception, ITestTimeoutException
		{ }

		[Fact]
		public void TimeoutExceptionTrumpsAssertionException()
		{
			var ex = new MyMultiException();

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Timeout, failed.Cause);
		}

		class MyMultiException : Exception, IAssertionException, ITestTimeoutException
		{ }
	}
}
