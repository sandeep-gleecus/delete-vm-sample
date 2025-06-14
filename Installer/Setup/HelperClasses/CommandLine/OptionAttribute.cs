﻿using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	/// <summary>Models an option specification.</summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class OptionAttribute : BaseAttribute
	{
		private string setName;

		private OptionAttribute(string shortName, string longName) : base()
		{
			if (shortName == null) throw new ArgumentNullException("shortName");
			if (longName == null) throw new ArgumentNullException("longName");

			ShortName = shortName;
			LongName = longName;
			setName = string.Empty;
			Separator = '\0';
		}

		/// <summary>Initializes a new instance of the <see cref="CommandLine.OptionAttribute"/> class.
		/// The default long name will be inferred from target property.</summary>
		public OptionAttribute()
			: this(string.Empty, string.Empty)
		{		}

		/// <summary>Initializes a new instance of the <see cref="CommandLine.OptionAttribute"/> class.</summary>
		/// <param name="longName">The long name of the option.</param>
		public OptionAttribute(string longName)
			: this(string.Empty, longName)
		{		}

		/// <summary>Initializes a new instance of the <see cref="CommandLine.OptionAttribute"/> class.</summary>
		/// <param name="shortName">The short name of the option.</param>
		/// <param name="longName">The long name of the option or null if not used.</param>
		public OptionAttribute(char shortName, string longName)
			: this(shortName.ToOneCharString(), longName)
		{		}

		/// <summary>Initializes a new instance of the <see cref="CommandLine.OptionAttribute"/> class.</summary>
		/// <param name="shortName">The short name of the option..</param>
		public OptionAttribute(char shortName)
			: this(shortName.ToOneCharString(), string.Empty)
		{		}

		/// <summary>Gets long name of this command line option. This name is usually a single english word.</summary>
		public string LongName { get; }

		/// <summary>Gets a short name of this command line option, made of one character.</summary>
		public string ShortName { get; }

		/// <summary>Gets or sets the option's mutually exclusive set name.</summary>
		public string SetName
		{
			get { return setName; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");

				setName = value;
			}
		}

		/// <summary>When applying attribute to <see cref="System.Collections.Generic.IEnumerable{T}"/> target properties,
		/// it allows you to split an argument and consume its content as a sequence.</summary>
		public char Separator { get; set; }
	}
}