using CSharpx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	enum SpecificationType
	{
		Option,
		Value
	}

	enum TargetType
	{
		Switch,
		Scalar,
		Sequence
	}

	abstract class Specification
	{
		protected Specification(SpecificationType tag, bool required, Maybe<int> min, Maybe<int> max,
			Maybe<object> defaultValue, string helpText, string metaValue, IEnumerable<string> enumValues,
			Type conversionType, TargetType targetType, bool hidden = false)
		{
			Tag = tag;
			Required = required;
			Min = min;
			Max = max;
			DefaultValue = defaultValue;
			ConversionType = conversionType;
			TargetType = targetType;
			HelpText = helpText;
			MetaValue = metaValue;
			EnumValues = enumValues;
			Hidden = hidden;
		}

		public SpecificationType Tag { get; }

		public bool Required { get; }

		public Maybe<int> Min { get; }

		public Maybe<int> Max { get; }

		public Maybe<object> DefaultValue { get; }

		public string HelpText { get; }

		public string MetaValue { get; }

		public IEnumerable<string> EnumValues { get; }

		public Type ConversionType { get; }

		public TargetType TargetType { get; }

		public bool Hidden { get; }

		public static Specification FromProperty(PropertyInfo property)
		{
			var attrs = property.GetCustomAttributes(true);
			var oa = attrs.OfType<OptionAttribute>();
			if (oa.Count() == 1)
			{
				var spec = OptionSpecification.FromAttribute(oa.Single(), property.PropertyType,
					property.PropertyType.GetTypeInfo().IsEnum
						? Enum.GetNames(property.PropertyType)
						: Enumerable.Empty<string>());
				if (spec.ShortName.Length == 0 && spec.LongName.Length == 0)
				{
					return spec.WithLongName(property.Name.ToLowerInvariant());
				}
				return spec;
			}

			var va = attrs.OfType<ValueAttribute>();
			if (va.Count() == 1)
			{
				return ValueSpecification.FromAttribute(va.Single(), property.PropertyType,
					property.PropertyType.GetTypeInfo().IsEnum
						? Enum.GetNames(property.PropertyType)
						: Enumerable.Empty<string>());
			}

			throw new InvalidOperationException();
		}
	}
}
