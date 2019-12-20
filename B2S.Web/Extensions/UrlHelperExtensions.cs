using B2S.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace B2S.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(UserAccessController.ConfirmEmail),
                controller: "UserAccess",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(UserAccessController.ResetPassword),
                controller: "UserAccess",
                values: new { userId, code },
                protocol: scheme);
        }
    }
}
