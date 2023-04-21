using UserApiTestTask.Application.Common.Interfaces;

namespace UserApiTestTask.Api.Services;

/// <inheritdoc/>
public class DateTimeProvider : IDateTimeProvider
{
	/// <inheritdoc/>
	public DateTime UtcNow => DateTime.UtcNow;
}
