using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Irish_Beauty_Product.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var role = session.GetString("Role");
            var username = session.GetString("Username");

            //  If user is not logged in
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            //  If user role is not authorized for this page
            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(role))
            {
                // Optional: you can show a custom AccessDenied page if you want
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
