using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApiTestTask.Api.AuthorizationAttributes;
using UserApiTestTask.Application.Users.Commands.DeleteUser;
using UserApiTestTask.Application.Users.Commands.PutUser;
using UserApiTestTask.Application.Users.Queries.GetActiveUsers;
using UserApiTestTask.Application.Users.Queries.GetOlderThanGivenAgeUsers;
using UserApiTestTask.Application.Users.Queries.GetUser;
using UserApiTestTask.Contracts.Requests.Users.GetUser;
using UserApiTestTask.Contracts.Requests.Users.GetUsers;
using UserApiTestTask.Contracts.Requests.Users.PutUser;

namespace UserApiTestTask.Api.Controllers;

/// <summary>
/// Контроллер для Пользователей
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="mediator">Медиатор</param>
	public UserController(IMediator mediator)
		=> _mediator = mediator;

	/// <summary>
	/// Получить данные о пользователе
	/// </summary>
	/// <remarks>Доступно только администратору, либо лично пользователю, если он активен</remarks>
	/// <param name="login">Логин</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Данные о пользователе</returns>
	[HttpGet("{login}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<GetUserResponse> GetAsync(
		[FromRoute] string login,
		CancellationToken cancellationToken = default)
			=> await _mediator.Send(new GetUserQuery(login), cancellationToken);

	/// <summary>
	/// Получить активных пользователей
	/// </summary>
	/// <remarks>Доступно только администратору</remarks>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Активных пользователей</returns>
	[HttpGet("ActiveUsers")]
	[Authorize]
	[AdminAuthorization]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<GetUsersResponse> GetActiveUsersAsync(CancellationToken cancellationToken = default)
			=> await _mediator.Send(new GetActiveUsersQuery(), cancellationToken);

	/// <summary>
	/// Получить пользователей старше заданного возраста
	/// </summary>
	/// <remarks>Доступно только администратору</remarks>
	/// <param name="age">Возраст, старше которого должны быть пользователи</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Пользователей старше заданного возраста</returns>
	[HttpGet("UsersOlderThan/{age}")]
	[Authorize]
	[AdminAuthorization]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<GetUsersResponse> GetActiveUsersAsync(
		[FromRoute] int age,
		CancellationToken cancellationToken = default)
			=> await _mediator.Send(new GetOlderThanGivenAgeUsersQuery(age), cancellationToken);

	/// <summary>
	/// Обновить данные о пользователе
	/// </summary>
	/// <remarks>Доступно только администратору, либо лично пользователю, если он активен</remarks>
	/// <param name="login">Логин</param>
	/// <param name="request">Запрос</param>
	/// <param name="cancellationToken">Токен отмены</param>
	[HttpPut("{login}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task PutAsync(
		[FromRoute] string login,
		PutUserRequest request,
		CancellationToken cancellationToken)
			=> await _mediator.Send(
				new PutUserCommand(login)
				{
					Name = request.Name,
					Gender = request.Gender,
					BirthDay = request.BirthDay,
				},
				cancellationToken);

	/// <summary>
	/// Удалить пользователя
	/// </summary>
	/// <remarks>Доступно только администратору</remarks>
	/// <param name="login">Логин</param>
	/// <param name="withSoftDelete">>Использовать мягкое удаление</param>
	/// <param name="cancellationToken">Токен отмены</param>
	[HttpDelete("{login}")]
	[Authorize]
	[AdminAuthorization]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task DeleteAsync(
		[FromRoute] string login,
		[FromQuery] bool withSoftDelete = true,
		CancellationToken cancellationToken = default)
			=> await _mediator.Send(
				new DeleteUserCommand
				{
					Login = login,
					WithSoftDelete = withSoftDelete,
				},
				cancellationToken);
}
