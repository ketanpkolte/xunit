using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This message indicates that a test case had been found during the discovery process.
/// </summary>
[JsonTypeID("test-case-discovered")]
public sealed class TestCaseDiscovered : TestCaseMessage, ITestCaseMetadata, IWritableTestCaseMetadata
{
	string? serialization;
	string? testCaseDisplayName;
	string? testClassName;
	IReadOnlyDictionary<string, IReadOnlyList<string>>? traits;

	/// <summary>
	/// Gets the serialized value of the test case, which allows it to be transferred across
	/// process boundaries.
	/// </summary>
	public required string Serialization
	{
		get => this.ValidateNullablePropertyValue(serialization, nameof(Serialization));
		set => serialization = Guard.ArgumentNotNull(value, nameof(Serialization));
	}

	/// <inheritdoc/>
	public required string? SkipReason { get; set; }

	/// <inheritdoc/>
	public required string? SourceFilePath { get; set; }

	/// <inheritdoc/>
	public required int? SourceLineNumber { get; set; }

	/// <inheritdoc/>
	public required string TestCaseDisplayName
	{
		get => this.ValidateNullablePropertyValue(testCaseDisplayName, nameof(TestCaseDisplayName));
		set => testCaseDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCaseDisplayName));
	}

	/// <inheritdoc/>
	[NotNullIfNotNull(nameof(TestMethodName))]
	public required string? TestClassName
	{
		get
		{
			if (testClassName is null && TestMethodName is not null)
				throw new InvalidOperationException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Illegal null {0} on an instance of '{1}' when {2} is not null",
						nameof(TestClassName),
						GetType().SafeName(),
						nameof(TestMethodName)
					)
				);

			return testClassName;
		}
		set => testClassName = value;
	}

	/// <inheritdoc/>
	public required string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	public required string? TestMethodName { get; set; }

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string ITestCaseMetadata.UniqueID =>
		TestCaseUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestCaseMetadata(this);

		serialization = JsonDeserializer.TryGetString(root, nameof(Serialization));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeTestCaseMetadata(this);

		serializer.Serialize(nameof(Serialization), Serialization);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1}", base.ToString(), testCaseDisplayName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(serialization, nameof(Serialization), invalidProperties);
		ValidatePropertyIsNotNull(testCaseDisplayName, nameof(TestCaseDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);

		if (TestMethodName is not null)
			ValidatePropertyIsNotNull(testClassName, nameof(TestClassName), invalidProperties);
	}
}
