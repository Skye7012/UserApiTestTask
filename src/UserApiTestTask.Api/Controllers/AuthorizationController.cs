using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApiTestTask.Application.Authorization.Commands.PutUserLogin;
using UserApiTestTask.Application.Authorization.Commands.PutUserPassword;
using UserApiTestTask.Application.Authorization.Commands.Refresh;
using UserApiTestTask.Application.Authorization.Commands.SignIn;
using UserApiTestTask.Application.Authorization.Commands.SignOut;
using UserApiTestTask.Application.Authorization.Commands.SignUp;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserLogin;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserPassword;
using UserApiTestTask.Contracts.Requests.Authorization.Refresh;
using UserApiTestTask.Contracts.Requests.Authorization.SignIn;
using UserApiTestTask.Contracts.Requests.Authorization.SignUp;

namespace UserApiTestTask.Api.Controllers;

/// <summary>
/// Контроллер авторизации
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthorizationController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="mediator">Медиатор</param>
	public AuthorizationController(IMediator mediator)
		=> _mediator = mediator;

	/// <summary>
	/// Зарегистрироваться (создать пользователя)
	/// </summary>
	/// <remarks>Доступно только администратору</remarks>
	/// <param name="request">Запрос</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Идентификатор созданного пользователя</returns>
	[HttpPost("SignUp")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<SignUpResponse>> SignUpAsync(
		SignUpRequest request,
		CancellationToken cancellationToken)
			=> CreatedAtAction(
				nameof(UserController.GetAsync),
				"User",
				new { login = request.Login },
				await _mediator.Send(
					new SignUpCommand
					{
						Login = request.Login,
						Name = request.Name,
						Password = request.Password,
						BirthDay = request.BirthDay,
						Gender = request.Gender,
						IsAdmin = request.IsAdmin,
					},
					cancellationToken));

	/// <summary>
	/// Авторизоваться
	/// </summary>
	/// <param name="request">Запрос</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Токены авторизации</returns>
	[HttpPost("SignIn")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<SignInResponse> SignInAsync(
		SignInRequest request,
		CancellationToken cancellationToken)
		=> await _mediator.Send(
			new SignInCommand
			{
				Login = request.Login,
				Password = request.Password,
			},
			cancellationToken);

	/// <summary>
	/// Обновить токены
	/// </summary>
	/// <param name="request">Запрос</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Обновленные токены</returns>
	[HttpPost("Refresh")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<RefreshTokenResponse> RefreshAsync(
		RefreshTokenRequest request,
		CancellationToken cancellationToken)
		=> await _mediator.Send(
			new RefreshTokenCommand
			{
				RefreshToken = request.RefreshToken,
			},
			cancellationToken);

	/// <summary>
	/// Выйти (завершить все сеансы)
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Обновленные токены</returns>
	[HttpDelete("SignOut")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task SignOutAsync(CancellationToken cancellationToken)
		=> await _mediator.Send(
			new SignOutCommand(),
			cancellationToken);

	/// <summary>
	/// Изменить пароль аккаунта пользователя
	/// </summary>
	/// <remarks>Доступно администратору, либо лично пользователю</remarks>
	/// <param name="login">Логин</param>
	/// <param name="request">Запрос</param>
	/// <param name="cancellationToken">Токен отмены</param>
	[HttpPut("ChangePassword/{login}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<PutUserPasswordResponse> PutPasswordAsync(
		[FromRoute] string login,
		PutUserPasswordRequest request,
		CancellationToken cancellationToken)
			=> await _mediator.Send(
				new PutUserPasswordCommand(login)
				{
					NewPassword = request.NewPassword,
					OldPassword = request.OldPassword,
				},
				cancellationToken);

	/// <summary>
	/// Изменить логин аккаунта пользователя
	/// </summary>
	/// <remarks>Доступно администратору, либо лично пользователю</remarks>
	/// <param name="login">Логин</param>
	/// <param name="request">Запрос</param>
	/// <param name="cancellationToken">Токен отмены</param>
	[HttpPut("ChangeLogin/{login}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<PutUserLoginResponse> PutLoginAsync(
		[FromRoute] string login,
		PutUserLoginRequest request,
		CancellationToken cancellationToken)
			=> await _mediator.Send(
				new PutUserLoginCommand(login)
				{
					NewLogin = request.NewLogin,
				},
				cancellationToken);
}
