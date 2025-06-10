using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	static class Sequence
	{
		public static IEnumerable<Token> Partition(
			IEnumerable<Token> tokens,
			Func<string, Maybe<TypeDescriptor>> typeLookup)
		{
			return from tseq in tokens.Pairwise(
				(f, s) =>
						f.IsName() && s.IsValue()
							? typeLookup(f.Text).MapValueOrDefault(info =>
								   info.TargetType == TargetType.Sequence
										? new[] { f }.Concat(tokens.OfSequence(f, info))
										: new Token[] { }, new Token[] { })
							: new Token[] { })
				   from t in tseq
				   select t;
		}

		private static IEnumerable<Token> OfSequence(this IEnumerable<Token> tokens, Token nameToken, TypeDescriptor info)
		{
			var nameIndex = tokens.IndexOf(t => t.Equals(nameToken));
			if (nameIndex >= 0)
			{
				return info.NextValue.MapValueOrDefault(
					_ => info.MaxItems.MapValueOrDefault(
							n => tokens.Skip(nameIndex + 1).Take(n),
								 tokens.Skip(nameIndex + 1).TakeWhile(v => v.IsValue())),
					tokens.Skip(nameIndex + 1).TakeWhile(v => v.IsValue()));
			}
			return new Token[] { };
		}
	}
}
