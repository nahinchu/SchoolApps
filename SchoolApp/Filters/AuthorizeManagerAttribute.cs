using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace SchoolApp.Filters
{
    public class AuthorizeManagerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var role = session.GetString("Role");
            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if(string.IsNullOrEmpty(role))
            {
                if (isAjax)
                    context.Result = new StatusCodeResult(401); // Unauthorized for AJAX requests
                else
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            bool isAllowed = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) 
                || role.Equals("Manager", StringComparison.OrdinalIgnoreCase);
            if(!isAllowed)
            {
                if (isAjax)
                    context.Result = new StatusCodeResult(403); // Forbidden for AJAX requests
                else
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
