using System.Text.Json;
using System.Text.Json.Serialization;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Api.JsonConverters;

/// <summary>
/// Более строгая версия конвертера <see cref="JsonStringEnumConverter"/>
/// , которая проверяет, что значение входит в перечисление
/// </summary>
public class StrictJsonStringEnumConverter : JsonConverterFactory
{
	private readonly JsonStringEnumConverter _internalFactory;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="namingPolicy">Политика наименования</param>
	/// <param name="allowIntegerValues">Можно ли использовать числа</param>
	public StrictJsonStringEnumConverter(
		JsonNamingPolicy? namingPolicy = null,
		bool allowIntegerValues = true)
			=> _internalFactory = new JsonStringEnumConverter(namingPolicy, allowIntegerValues);

	/// <inheritdoc/>
	public override bool CanConvert(Type typeToConvert)
		=> _internalFactory.CanConvert(typeToConvert);

	/// <inheritdoc/>
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var jsonConverter = typeof(JsonConverter<>).MakeGenericType(typeToConvert);
		var converterInstance = (JsonConverter)typeof(StrictEnumJsonConverter<>)
			.MakeGenericType(typeToConvert)
			.GetConstructor(new[] { jsonConverter })!
			.Invoke(new[] { _internalFactory.CreateConverter(typeToConvert, options) });

		return converterInstance;
	}
}


/// <summary>
/// JSON конвертер со строгой проверкой вхождения значения в enum
/// </summary>
public class StrictEnumJsonConverter<T> : JsonConverter<T>
	where T : Enum
{
	private readonly JsonConverter<T> _innerConverter;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="innerConverter">Базовый конвертер</param>
	public StrictEnumJsonConverter(JsonConverter<T> innerConverter)
		=> _innerConverter = innerConverter;

	/// <inheritdoc/>
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert.IsEnum;

	/// <inheritdoc/>
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var result = _innerConverter.Read(ref reader, typeToConvert, options)
			?? throw new ApplicationProblem("Не получилось спарсить enum");

		return !Enum.IsDefined(typeToConvert, result)
			? throw new ValidationProblem($"Значение '{result}' не входит в перечисление {typeToConvert.Name}")
			: result;
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		=> _innerConverter.Write(writer, value, options);
}
