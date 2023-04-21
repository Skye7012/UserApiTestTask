using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApiTestTask.Application.Authorization.Commands.SignIn;
using UserApiTestTask.Application.Authorization.Commands.SignUp;
using UserApiTestTask.Application.Common.Static;
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
	/// Зарегистрироваться
	/// </summary>
	/// <param name="request">Запрос</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Идентификатор созданного пользователя</returns>
	[HttpPost("SignUp")]
	[Authorize(Policy = CustomPolicies.IsAdminClaimPolicy)]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
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
	/// <returns>Authorization token</returns>
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
}
