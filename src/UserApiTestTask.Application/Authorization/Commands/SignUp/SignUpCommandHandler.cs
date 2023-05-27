using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Requests.Authorization.SignUp;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Authorization.Commands.SignUp;

/// <summary>
/// Обработчик для <see cref="SignUpCommand"/>
/// </summary>
public class SignUpCommandHandler : IRequestHandler<SignUpCommand, SignUpResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly IPasswordService _passwordService;
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="passwordService">Сервис паролей</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	public SignUpCommandHandler(
		IApplicationDbContext context,
		IPasswordService passwordService,
		IAuthorizationService authorizationService)
	{
		_context = context;
		_passwordService = passwordService;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task<SignUpResponse> Handle(SignUpCommand request, CancellationToken cancellationToken)
	{
		_authorizationService.CheckIsAdmin();

		var isLoginUnique = await _context.Users
			.AllAsync(x => x.UserAccount!.Login != request.Login, cancellationToken);

		if (!isLoginUnique)
			throw new ValidationProblem("Пользователь с таким логином уже существует");

		if (!Regex.IsMatch(request.Password, @"^[a-zA-Z0-9]+$"))
			throw new ValidationProblem("Для пароля запрещены все символы кроме латинских букв и цифр");

		_passwordService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

		var user = new User(
			name: request.Name,
			gender: request.Gender,
			birthDay: request.BirthDay,
			isAdmin: request.IsAdmin,
			userAccount: new UserAccount(
				login: request.Login,
				passwordHash: passwordHash,
				passwordSalt: passwordSalt));

		await _context.Users.AddAsync(user, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		return new SignUpResponse()
		{
			UserId = user.Id,
		};
	}
}
