using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using ValidationException = Shared.Common.Exceptions.ValidationException;

namespace Shared.Common.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .ToArray();

                throw new ValidationException(errors);
            }
        }

        await next();
    }
}