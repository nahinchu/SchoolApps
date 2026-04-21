using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SchoolApp.Filters
{
    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var role = session.GetString("Role");
            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (string.IsNullOrEmpty(role))
            {
                // Chưa login
                if (isAjax)
                    context.Result = new StatusCodeResult(401);
                else
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (!role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                // Đã login nhưng không phải admin
                if (isAjax)
                    context.Result = new JsonResult(new { success = false, message = "Bạn không có quyền Admin" })
                    {
                        StatusCode = 403
                    };
                else
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}