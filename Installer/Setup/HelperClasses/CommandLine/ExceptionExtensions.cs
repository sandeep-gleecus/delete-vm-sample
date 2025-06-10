using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	static class ExceptionExtensions
	{
		public static void RethrowWhenAbsentIn(this Exception exception, IEnumerable<Type> validExceptions)
		{
			if (!validExceptions.Contains(exception.GetType()))
			{
				throw exception;
			}
		}
	}
}
