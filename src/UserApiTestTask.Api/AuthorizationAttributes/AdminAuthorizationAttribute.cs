using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Static;

namespace UserApiTestTask.Api.AuthorizationAttributes;

/// <summary>
/// Атрибут для авторизации по роли Администратор
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminAuthorizationAttribute : Attribute, IAuthorizationFilter
{
	/// <inheritdoc/>
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var isAdmin = bool.Parse(
			context.HttpContext.User.FindFirstValue(CustomClaims.IsAdminClaimName) ?? "false");

		if (!isAdmin)
			throw new ForbiddenProblem("Данное действие доступно только для администраторов");
	}
}
