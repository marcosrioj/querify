using Microsoft.AspNetCore.Mvc.Filters;

namespace Querify.Common.Infrastructure.Mvc.Filters;

public class StringTrimmingActionFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            TrimStrings(argument);
        }
    }

    private void TrimStrings(object? model)
    {
        if (model == null) return;

        var properties = model.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

        foreach (var property in properties)
        {
            if (property.GetValue(model) is string value)
            {
                property.SetValue(model, value.Trim());
            }
        }
    }
}