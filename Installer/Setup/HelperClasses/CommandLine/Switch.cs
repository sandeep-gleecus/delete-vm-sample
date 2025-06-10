using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	static class Switch
	{
		public static IEnumerable<Token> Partition(
			IEnumerable<Token> tokens,
			Func<string, Maybe<TypeDescriptor>> typeLookup)
		{
			return from t in tokens
				   where typeLookup(t.Text).MapValueOrDefault(info => t.IsName() && info.TargetType == TargetType.Switch, false)
				   select t;
		}
	}
}
