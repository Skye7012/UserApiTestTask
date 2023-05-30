using System.Net.Http;
using System.Threading.Tasks;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserLogin;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserPassword;
using UserApiTestTask.Contracts.Requests.Authorization.Refresh;
using UserApiTestTask.Contracts.Requests.Authorization.SignIn;
using UserApiTestTask.Contracts.Requests.Authorization.SignUp;
using UserApiTestTask.Contracts.Requests.Users.GetUser;
using UserApiTestTask.Contracts.Requests.Users.GetUsers;
using UserApiTestTask.Contracts.Requests.Users.PutUser;
using UserApiTestTask.IntegrationTests.Common;

namespace UserApiTestTask.IntegrationTests.Extensions;

/// <summary>
/// Расширения для <see cref="HttpClient"/>
/// </summary>
public static class HttpClientExtensions
{
	/// <summary>
	/// Зарегистрироваться
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="request">Запрос</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<SignUpResponse>> SignUpAsync(
		this HttpClient client,
		SignUpRequest request)
	{
		var response = await client
			.PostAsJsonAsync(
				"/Authorization/SignUp",
				request);

		return await HttpResponseMessageWithResult<SignUpResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Аутентифицироваться
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="request">Запрос</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<SignInResponse>> SignInAsync(
		this HttpClient client,
		SignInRequest request)
	{
		var response = await client
			.PostAsJsonAsync("/Authorization/SignIn", request);

		return await HttpResponseMessageWithResult<SignInResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Обновить токены
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="request">Запрос</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<RefreshTokenResponse>> RefreshTokensAsync(
		this HttpClient client,
		RefreshTokenRequest request)
	{
		var response = await client
			.PostAsJsonAsync("/Authorization/Refresh", request);

		return await HttpResponseMessageWithResult<RefreshTokenResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Завершить сессии
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessage> SignOutAsync(
		this HttpClient client)
			=> await client.DeleteAsync("/Authorization/SignOut");

	/// <summary>
	/// Изменить пароль акканута пользователя
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="login">Логин</param>
	/// <param name="request">Запрос</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<PutUserPasswordResponse>> ChangePasswordAsync(
		this HttpClient client,
		string login,
		PutUserPasswordRequest request)
	{
		var response = await client
			.PutAsJsonAsync($"/Authorization/ChangePassword/{login}", request);

		return await HttpResponseMessageWithResult<PutUserPasswordResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Изменить логин акканута пользователя
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="login">Логин</param>
	/// <param name="request">Запрос</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<PutUserLoginResponse>> ChangeLoginAsync(
		this HttpClient client,
		string login,
		PutUserLoginRequest request)
	{
		var response = await client
			.PutAsJsonAsync($"/Authorization/ChangeLogin/{login}", request);

		return await HttpResponseMessageWithResult<PutUserLoginResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Получить информацию о пользователе
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="login">Логин</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<GetUserResponse>> GetUserAsync(
		this HttpClient client,
		string login)
	{
		var response = await client
			.GetAsync($"/User/{login}");

		return await HttpResponseMessageWithResult<GetUserResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Получить информацию о пользователях, которые активны
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<GetUsersResponse>> GetActiveUsersAsync(
		this HttpClient client)
	{
		var response = await client
			.GetAsync($"/User/ActiveUsers");

		return await HttpResponseMessageWithResult<GetUsersResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Получить информацию о пользователях, которые старше заданного возраста
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="age">Возраст</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessageWithResult<GetUsersResponse>> GetGetOlderThanGivenAgeUsersAsync(
		this HttpClient client,
		int age)
	{
		var response = await client
			.GetAsync($"/User/UsersOlderThan/{age}");

		return await HttpResponseMessageWithResult<GetUsersResponse>.CreateAsync(response);
	}

	/// <summary>
	/// Изменить данные пользователя
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="login">Логин</param>
	/// <param name="request">Запрос</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessage> PutUserAsync(
		this HttpClient client,
		string login,
		PutUserRequest request)
			=> await client
				.PutAsJsonAsync($"/User/{login}", request);

	/// <summary>
	/// Восстановить пользователя
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="login">Логин</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessage> RestoreUserAsync(
		this HttpClient client,
		string login)
			=> await client
				.PutAsync($"/User/Restore/{login}", null);

	/// <summary>
	/// Удалить пользователя
	/// </summary>
	/// <param name="client">Клиент</param>
	/// <param name="login">Логин</param>
	/// <returns>Ответ</returns>
	public static async Task<HttpResponseMessage> DeleteUserAsync(
		this HttpClient client,
		string login)
			=> await client.DeleteAsync($"/User/{login}");
}
