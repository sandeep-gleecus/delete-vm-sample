﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	static class TokenPartitioner
	{
		public static
			Tuple<IEnumerable<KeyValuePair<string, IEnumerable<string>>>, IEnumerable<string>, IEnumerable<Token>> Partition(
				IEnumerable<Token> tokens,
				Func<string, Maybe<TypeDescriptor>> typeLookup)
		{
			IEqualityComparer<Token> tokenComparer = ReferenceEqualityComparer.Default;

			var tokenList = tokens.Memorize();
			var switches = new HashSet<Token>(Switch.Partition(tokenList, typeLookup), tokenComparer);
			var scalars = new HashSet<Token>(Scalar.Partition(tokenList, typeLookup), tokenComparer);
			var sequences = new HashSet<Token>(Sequence.Partition(tokenList, typeLookup), tokenComparer);
			var nonOptions = tokenList
				.Where(t => !switches.Contains(t))
				.Where(t => !scalars.Contains(t))
				.Where(t => !sequences.Contains(t)).Memorize();
			var values = nonOptions.Where(v => v.IsValue()).Memorize();
			var errors = nonOptions.Except(values, (IEqualityComparer<Token>)ReferenceEqualityComparer.Default).Memorize();

			return Tuple.Create(
					KeyValuePairHelper.ForSwitch(switches)
						.Concat(KeyValuePairHelper.ForScalar(scalars))
						.Concat(KeyValuePairHelper.ForSequence(sequences)),
				values.Select(t => t.Text),
				errors);
		}
	}
}