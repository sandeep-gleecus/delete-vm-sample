using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	enum TokenType { Name, Value }

	abstract class Token
	{
		protected Token(TokenType tag, string text)
		{
			Tag = tag;
			Text = text;
		}

		public static Token Name(string text)
		{
			return new Name(text);
		}

		public static Token Value(string text)
		{
			return new Value(text);
		}

		public static Token Value(string text, bool explicitlyAssigned)
		{
			return new Value(text, explicitlyAssigned);
		}

		public TokenType Tag { get; }

		public string Text { get; }
	}

	class Name : Token, IEquatable<Name>
	{
		public Name(string text)
			: base(TokenType.Name, text)
		{ }

		public override bool Equals(object obj)
		{
			var other = obj as Name;
			if (other != null)
			{
				return Equals(other);
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return new { Tag, Text }.GetHashCode();
		}

		public bool Equals(Name other)
		{
			if (other == null)
			{
				return false;
			}

			return Tag.Equals(other.Tag) && Text.Equals(other.Text);
		}
	}

	class Value : Token, IEquatable<Value>
	{
		private readonly bool explicitlyAssigned;

		public Value(string text)
			: this(text, false)
		{ }

		public Value(string text, bool explicitlyAssigned)
			: base(TokenType.Value, text)
		{
			this.explicitlyAssigned = explicitlyAssigned;
		}

		public bool ExplicitlyAssigned
		{
			get { return explicitlyAssigned; }
		}

		public override bool Equals(object obj)
		{
			var other = obj as Value;
			if (other != null)
			{
				return Equals(other);
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return new { Tag, Text }.GetHashCode();
		}

		public bool Equals(Value other)
		{
			if (other == null)
			{
				return false;
			}

			return Tag.Equals(other.Tag) && Text.Equals(other.Text);
		}
	}

	static class TokenExtensions
	{
		public static bool IsName(this Token token)
		{
			return token.Tag == TokenType.Name;
		}

		public static bool IsValue(this Token token)
		{
			return token.Tag == TokenType.Value;
		}
	}
}