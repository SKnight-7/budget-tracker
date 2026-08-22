using System.Diagnostics.CodeAnalysis;
using BudgetTracker.Infrastructure;
using System.Text.Json;
using BudgetTracker.Models;

namespace BudgetTracker.Repositories;

/// <summary>
/// The JSON implementation of <see cref="IBudgetRepository"/>: keeps the
/// budgets in a JSON file inside the StatePersistence folder, one document
/// with the same property names and nesting as TrackedBudgets itself.
/// </summary>
public class JsonBudgetRepository : IBudgetRepository
{
    /// <summary>The path to where the budgets file is stored on the local
    /// computer, computed from the state persistence folder and
    /// PersistenceFileName on every read, so the path can never fall out of
    /// step with PersistenceFileName.</summary>
    private string PersistenceFilePath => Path.Combine(FolderPaths.StatePersistence, PersistenceFileName);

    /// <summary>Serializer options, built once and reused: indented output,
    /// so the stored file is readable by humans.</summary>
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    private string _persistenceFileName;

    /// <summary>
    /// The constructor assigns through the property, so its checks run during
    /// construction too: a repository can't be created with a filename the
    /// setter would reject.
    /// </summary>
    public JsonBudgetRepository(string persistenceFileName = "currentBudgets.json")
    {
        PersistenceFileName = persistenceFileName;
    }

    /// <summary>The name of the JSON file this repository reads from and writes to.</summary>
    /// <exception cref="ArgumentNullException">Thrown when set to null.</exception>
    /// <exception cref="ArgumentException">Thrown when set to an empty string or to a
    /// name that doesn't end in ".json".</exception>
    public string PersistenceFileName
    {
        get => _persistenceFileName;

        [MemberNotNull(nameof(_persistenceFileName))]
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(PersistenceFileName));
            if (!value.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("File must be in JSON format", nameof(PersistenceFileName));
            _persistenceFileName = value;
        }
    }

    /// <inheritdoc/>
    /// <remarks>Reads the whole file and deserializes it as one set of
    /// budgets. Tolerant wherever nothing can be lost: a missing or blank
    /// file, a file holding only the word null, or a set with zero
    /// categories loads as null, the same as nothing stored, so the
    /// manager's defaults policy applies. Damage is different: text that
    /// cannot be parsed as budgets, including a document with no category
    /// list at all, stops the load with an exception, rather than letting a
    /// damaged file be mistaken for an empty one and overwritten.</remarks>
    /// <exception cref="InvalidDataException">Thrown when the file cannot be
    /// read as budget data, whether from malformed JSON or from values the
    /// models reject (the model constructors run during deserialization, so
    /// duplicate category names are refused here too). The original error
    /// stays attached as the InnerException.</exception>
    public TrackedBudgets? Load()
    {
        if (!File.Exists(PersistenceFilePath))
            return null;

        string json = File.ReadAllText(PersistenceFilePath);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        TrackedBudgets? budgets;

        try
        {
            budgets = JsonSerializer.Deserialize<TrackedBudgets>(json);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw new InvalidDataException(
                $"{PersistenceFileName} could not be read as budget data. " +
                "Review the file to investigate, or delete it to start over.", exception);
        }

        if (budgets is null || budgets.Categories.Count == 0)
            return null;

        return budgets;
    }

    /// <inheritdoc/>
    /// <remarks>Creates the StatePersistence folder when it doesn't exist yet,
    /// and rewrites the whole file every time: one indented JSON document,
    /// an object with a single property named Categories whose value is the
    /// array of categories.</remarks>
    public void Save(TrackedBudgets budgets)
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);

        string budgetsAsString = JsonSerializer.Serialize(budgets, IndentedJson);
        File.WriteAllText(PersistenceFilePath, budgetsAsString);
    }
}