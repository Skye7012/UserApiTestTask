using System.Text.RegularExpressions;
using MediatR;
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
	private readonly IUserService _userService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="userService">Сервис пользователя</param>
	public SignUpCommandHandler(IApplicationDbContext context, IUserService userService)
	{
		_context = context;
		_userService = userService;
	}

	/// <inheritdoc/>
	public async Task<SignUpResponse> Handle(SignUpCommand request, CancellationToken cancellationToken)
	{
		var isLoginUnique = _context.Users.All(x => x.Login != request.Login);
		if (!isLoginUnique)
			throw new ValidationProblem("Пользователь с таким логином уже существует");

		if (!Regex.IsMatch(request.Password, @"^[a-zA-Z0-9]+$"))
			throw new ValidationProblem("Для пароля запрещены все символы кроме латинских букв и цифр");

		_userService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

		var user = new User(
			login: request.Login,
			passwordHash: passwordHash,
			passwordSalt: passwordSalt,
			name: request.Name,
			gender: request.Gender,
			birthDay: request.BirthDay,
			isAdmin: request.IsAdmin);

		await _context.Users.AddAsync(user, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		return new SignUpResponse()
		{
			UserId = user.Id,
		};
	}
}
