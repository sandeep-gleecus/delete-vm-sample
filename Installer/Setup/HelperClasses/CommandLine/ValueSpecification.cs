﻿using System;
using System.Collections.Generic;
using CSharpx;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	sealed class ValueSpecification : Specification
	{
		public ValueSpecification(int index, string metaName, bool required, Maybe<int> min, Maybe<int> max, Maybe<object> defaultValue,
			string helpText, string metaValue, IEnumerable<string> enumValues,
			Type conversionType, TargetType targetType, bool hidden = false)
			: base(SpecificationType.Value, required, min, max, defaultValue, helpText, metaValue, enumValues, conversionType, targetType, hidden)
		{
			Index = index;
			MetaName = metaName;
		}

		public static ValueSpecification FromAttribute(ValueAttribute attribute, Type conversionType, IEnumerable<string> enumValues)
		{
			return new ValueSpecification(
				attribute.Index,
				attribute.MetaName,
				attribute.Required,
				attribute.Min == -1 ? Maybe.Nothing<int>() : Maybe.Just(attribute.Min),
				attribute.Max == -1 ? Maybe.Nothing<int>() : Maybe.Just(attribute.Max),
				attribute.Default.ToMaybe(),
				attribute.HelpText,
				attribute.MetaValue,
				enumValues,
				conversionType,
				conversionType.ToTargetType(),
				attribute.Hidden);
		}

		public int Index { get; }

		public string MetaName { get; }
	}
}