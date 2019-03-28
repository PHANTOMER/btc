using System;
using System.Linq;
using Btc.Api.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Btc.Api.Extensions
{
    public static class ValidationExtensions
    {
        private const string DotSeparator = ".";

        public static ValidationException FromModelState(this ModelStateDictionary modelState, string errorMessage = "")
        {
            return new ValidationException(errorMessage, modelState.ToDictionary(
                x => x.Key.Contains(DotSeparator)
                    ? x.Key.Substring(x.Key.IndexOf(DotSeparator, StringComparison.Ordinal) + 1)
                    : x.Key,
                x => x.Value.Errors.Select(e => e.ErrorMessage).ToList()));
        }
    }
}
