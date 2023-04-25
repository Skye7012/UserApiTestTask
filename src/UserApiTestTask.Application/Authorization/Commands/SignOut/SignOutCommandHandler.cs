using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Interfaces;

namespace UserApiTestTask.Application.Authorization.Commands.SignOut;

/// <summary>
/// Обработчик для <see cref="SignOutCommand"/>
/// </summary>
public class SignOutCommandHandler : IRequestHandler<SignOutCommand>
{
	private readonly IApplicationDbContext _context;
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService"></param>
	public SignOutCommandHandler(
		IApplicationDbContext context,
		IAuthorizationService authorizationService)
	{
		_context = context;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task Handle(SignOutCommand request, CancellationToken cancellationToken)
	{
		var refreshTokens = await _context.RefreshTokens
			.Where(x => x.UserAccountId == _authorizationService.GetUserAccountId())
			.ToListAsync(cancellationToken);

		_context.RefreshTokens.RemoveRange(refreshTokens);
		await _context.SaveChangesAsync(cancellationToken);
	}
}
