﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	static class PreprocessorGuards
	{
		public static IEnumerable<Func<IEnumerable<string>, IEnumerable<Error>>>
			Lookup(StringComparer nameComparer, bool autoHelp, bool autoVersion)
		{
			var list = new List<Func<IEnumerable<string>, IEnumerable<Error>>>();
			if (autoHelp)
				list.Add(HelpCommand(nameComparer));
			if (autoVersion)
				list.Add(VersionCommand(nameComparer));
			return list;
		}

		public static Func<IEnumerable<string>, IEnumerable<Error>> HelpCommand(StringComparer nameComparer)
		{
			return
				arguments =>
					nameComparer.Equals("--help", arguments.First())
						? new Error[] { new HelpRequestedError() }
						: Enumerable.Empty<Error>();
		}

		public static Func<IEnumerable<string>, IEnumerable<Error>> VersionCommand(StringComparer nameComparer)
		{
			return
				arguments =>
					nameComparer.Equals("--version", arguments.First())
						? new Error[] { new VersionRequestedError() }
						: Enumerable.Empty<Error>();
		}
	}
}
