using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	sealed class Verb
	{
		public Verb(string name, string helpText, bool hidden = false)
		{
            Name = name;
            HelpText = helpText;
			Hidden = hidden;
		}

		public string Name { get; }

		public string HelpText { get; }

		public bool Hidden { get; }

		public static Verb FromAttribute(VerbAttribute attribute)
		{
			return new Verb(
				attribute.Name,
				attribute.HelpText,
				attribute.Hidden
				);
		}

		public static IEnumerable<Tuple<Verb, Type>> SelectFromTypes(IEnumerable<Type> types)
		{
			return from type in types
				   let attrs = type.GetTypeInfo().GetCustomAttributes(typeof(VerbAttribute), true).ToArray()
				   where attrs.Length == 1
				   select Tuple.Create(
					   FromAttribute((VerbAttribute)attrs.Single()),
					   type);
		}
	}
}