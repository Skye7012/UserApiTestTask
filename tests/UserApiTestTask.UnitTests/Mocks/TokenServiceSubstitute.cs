using System;
using System.Collections.Generic;
using System.Security.Claims;
using NSubstitute;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.UnitTests.Common.Interfaces;

namespace UserApiTestTask.UnitTests.Mocks;

/// <summary>
/// Сервис JWT токенов для тестов
/// </summary>
public class TokenServiceSubstitute : ITokenService, ISubstitute<ITokenService>
{
	public const string AccessTokenName = "accessToken";
	public const string RefreshTokenName = "refreshToken";

	/// <inheritdoc/>
	public ITokenService Create()
		=> Substitute.ForPartsOf<TokenServiceSubstitute>();

	/// <inheritdoc/>
	public virtual string CreateAccessToken(UserAccount userAccount)
		=> AccessTokenName;

	/// <inheritdoc/>
	public virtual string CreateRefreshToken()
		=> RefreshTokenName;

	/// <inheritdoc/>
	public virtual string CreateToken(DateTime expires, IEnumerable<Claim>? claims = null)
		=> "token";
}
