// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	sealed class OptionSpecification : Specification
	{
		public OptionSpecification(string shortName, string longName, bool required, string setName, Maybe<int> min, Maybe<int> max,
			char separator, Maybe<object> defaultValue, string helpText, string metaValue, IEnumerable<string> enumValues,
			Type conversionType, TargetType targetType, bool hidden = false)
			: base(SpecificationType.Option, required, min, max, defaultValue, helpText, metaValue, enumValues, conversionType, targetType, hidden)
		{
			ShortName = shortName;
			LongName = longName;
			Separator = separator;
			SetName = setName;
		}

		public static OptionSpecification FromAttribute(OptionAttribute attribute, Type conversionType, IEnumerable<string> enumValues)
		{
			return new OptionSpecification(
				attribute.ShortName,
				attribute.LongName,
				attribute.Required,
				attribute.SetName,
				attribute.Min == -1 ? Maybe.Nothing<int>() : Maybe.Just(attribute.Min),
				attribute.Max == -1 ? Maybe.Nothing<int>() : Maybe.Just(attribute.Max),
				attribute.Separator,
				attribute.Default.ToMaybe(),
				attribute.HelpText,
				attribute.MetaValue,
				enumValues,
				conversionType,
				conversionType.ToTargetType(),
				attribute.Hidden);
		}

		public static OptionSpecification NewSwitch(string shortName, string longName, bool required, string helpText, string metaValue, bool hidden = false)
		{
			return new OptionSpecification(shortName, longName, required, string.Empty, Maybe.Nothing<int>(), Maybe.Nothing<int>(),
				'\0', Maybe.Nothing<object>(), helpText, metaValue, Enumerable.Empty<string>(), typeof(bool), TargetType.Switch, hidden);
		}

		public string ShortName { get; }

		public string LongName { get; }

		public char Separator { get; }

		public string SetName { get; }
	}
}