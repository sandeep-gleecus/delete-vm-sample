using System.Collections.Generic;
using System.Linq;
using CSharpx;
using RailwaySharp.ErrorHandling;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	static class ResultExtensions
	{
		public static IEnumerable<TMessage> SuccessfulMessages<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
		{
			if (result.Tag == ResultType.Ok)
			{
				var ok = (Ok<TSuccess, TMessage>)result;
				return ok.Messages;
			}
			return Enumerable.Empty<TMessage>();
		}

		public static Maybe<TSuccess> ToMaybe<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
		{
			if (result.Tag == ResultType.Ok)
			{
				var ok = (Ok<TSuccess, TMessage>)result;
				return Maybe.Just(ok.Success);
			}
			return Maybe.Nothing<TSuccess>();
		}
	}
}