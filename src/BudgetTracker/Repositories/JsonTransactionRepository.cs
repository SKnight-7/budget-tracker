using System.Diagnostics.CodeAnalysis;
using BudgetTracker.Infrastructure;
using System.Text.Json;
using BudgetTracker.Models;

namespace BudgetTracker.Repositories;

/// <summary>
/// The JSON implementation of <see cref="ITransactionRepository"/>: keeps the
/// batch in a JSON file inside the StatePersistence folder, one document whose
/// shape is TransactionBatch itself.
/// </summary>
public class JsonTransactionRepository : ITransactionRepository
{
    /// <summary>Where the transactions file lives on disk, computed from the state
    /// persistence folder and PersistenceFileName on every read so it can never fall
    /// out of step.</summary>
    private string PersistenceFilePath => Path.Combine(FolderPaths.StatePersistence, PersistenceFileName);

    /// <summary>Serializer options, built once and reused: indented output,
    /// so the stored file is readable by humans.</summary>
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    /// <summary>Recorded as the batch's source when no stored row carries one.</summary>
    private const string UnknownSourceName = "unknown";

    private string _persistenceFileName;

    /// <summary>
    /// The constructor assigns through the property, so its checks run during
    /// construction too: a repository can't be created with a filename the
    /// setter would reject.
    /// </summary>
    public JsonTransactionRepository(string persistenceFileName = "lastUploadedTransactions.json")
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
    /// <remarks>Reads the whole file and deserializes it as one batch. Tolerant
    /// wherever nothing can be lost: a missing, empty, or hollow file (no
    /// batch, no transaction list, or zero transactions) loads as null, the
    /// same as nothing stored, and a missing or blank source file name is
    /// recorded as "unknown". Damage is different: text that cannot be parsed
    /// as a batch stops the load with an exception, rather than letting a
    /// damaged file be mistaken for an empty one and overwritten.</remarks>
    /// <exception cref="InvalidDataException">Thrown when the file cannot be
    /// read as transaction data, whether from malformed JSON or from values
    /// the models reject (the model constructors run during deserialization).
    /// The original error stays attached as the InnerException.</exception>
    public TransactionBatch? Load()
    {
        if (!File.Exists(PersistenceFilePath))
            return null;

        string json = File.ReadAllText(PersistenceFilePath);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        TransactionBatch? batch;

        try
        {
            batch = JsonSerializer.Deserialize<TransactionBatch>(json);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw new InvalidDataException(
                $"{PersistenceFileName} could not be read as transaction data. " +
                "Review the file to investigate, or delete it to start over.", exception);
        }

        if (batch is null || batch.Transactions is null || batch.Transactions.Count == 0)
            return null;

        if (string.IsNullOrWhiteSpace(batch.SourceFileName))
            return batch with { SourceFileName = UnknownSourceName };

        return batch;
    }

    /// <inheritdoc/>
    /// <remarks>Creates the StatePersistence folder when it doesn't exist yet,
    /// and rewrites the whole file every time: one indented JSON document,
    /// with the source file name stored once at the top and never repeated
    /// per transaction.</remarks>
    public void Save(TransactionBatch batch)
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);

        string batchAsString = JsonSerializer.Serialize(batch, IndentedJson);
        File.WriteAllText(PersistenceFilePath, batchAsString);
    }
}