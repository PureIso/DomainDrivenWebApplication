using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DomainDrivenWebApplication.API.Middleware
{
    public class ServiceTypeFilter : IActionFilter
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceTypeFilter> _logger;

        public ServiceTypeFilter(IConfiguration configuration, ILogger<ServiceTypeFilter> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string serviceType = _configuration["SERVICE_TYPE"] ?? "default";
            string controllerName = context.Controller.GetType().Name;

            if (serviceType.Equals("reader", StringComparison.OrdinalIgnoreCase))
            {
                // Block writer actions in reader mode
                if (controllerName.Contains("School") && IsWriteAction(context))
                {
                    _logger.LogWarning("Action {ActionName} is not allowed in reader mode.", context.ActionDescriptor.DisplayName);
                    context.Result = new ForbidResult(); // Return Forbidden (403)
                }
            }
            else if (serviceType.Equals("writer", StringComparison.OrdinalIgnoreCase))
            {
                // Block read actions in writer mode
                if (controllerName.Contains("School") && IsReadAction(context))
                {
                    _logger.LogWarning("Action {ActionName} is not allowed in writer mode.", context.ActionDescriptor.DisplayName);
                    context.Result = new ForbidResult(); // Return Forbidden (403)
                }
            }
        }

        private bool IsWriteAction(ActionExecutingContext context)
        {
            // Actions that modify data: POST, PUT, DELETE
            return context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   context.HttpContext.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                   context.HttpContext.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsReadAction(ActionExecutingContext context)
        {
            return context.HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception == null)
            {
                _logger?.LogInformation("Action executed successfully: {ActionName}", context.ActionDescriptor.DisplayName);
            }
            else
            {
                _logger?.LogError(context.Exception, "Action {ActionName} failed with an exception", context.ActionDescriptor.DisplayName);
            }
        }
    }
}
