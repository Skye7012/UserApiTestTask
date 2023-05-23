namespace UserApiTestTask.UnitTests.Common.Interfaces;

/// <summary>
/// Substitute
/// </summary>
public interface ISubstitute<T>
	where T : class
{
	/// <summary>
	/// Создать substitute
	/// </summary>
	T Create();
}
