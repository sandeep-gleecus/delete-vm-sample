﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Reflection;
using CSharpx;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	class SpecificationProperty
	{
		private SpecificationProperty(Specification specification, PropertyInfo property, Maybe<object> value)
		{
			Property = property;
			Specification = specification;
			Value = value;
		}

		public static SpecificationProperty Create(Specification specification, PropertyInfo property, Maybe<object> value)
		{
			if (value == null) throw new ArgumentNullException("value");

			return new SpecificationProperty(specification, property, value);
		}

		public Specification Specification { get; }

		public PropertyInfo Property { get; }

		public Maybe<object> Value { get; }
	}
}
