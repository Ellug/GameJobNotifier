using GameJobNotifier.App.Infrastructure;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using Microsoft.Data.Sqlite;

namespace GameJobNotifier.App.Services;

public sealed class SqliteJobPostingRepository : IJobPostingRepository
{
    private const int MaxIdsPerBatch = 500;
    private const string TableName = "job_postings";

    private readonly SemaphoreSlim _initGate = new(1, 1);
    private bool _initialized;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _initGate.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            AppPaths.EnsureCreated();

            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            await EnsureSchemaAsync(connection, cancellationToken);

            _initialized = true;
        }
        finally
        {
            _initGate.Release();
        }
    }

    public async Task<IReadOnlyDictionary<string, JobPostingRecord>> GetByIdsAsync(
        string sourceUrl,
        IReadOnlyCollection<string> jobIds,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            throw new ArgumentException("Source scope key is required.", nameof(sourceUrl));
        }

        if (jobIds.Count == 0)
        {
            return new Dictionary<string, JobPostingRecord>(StringComparer.Ordinal);
        }

        var normalizedIds = jobIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedIds.Length == 0)
        {
            return new Dictionary<string, JobPostingRecord>(StringComparer.Ordinal);
        }

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var records = new Dictionary<string, JobPostingRecord>(StringComparer.Ordinal);
        for (var offset = 0; offset < normalizedIds.Length; offset += MaxIdsPerBatch)
        {
            var batchSize = Math.Min(MaxIdsPerBatch, normalizedIds.Length - offset);

            await using var command = connection.CreateCommand();
            var parameterNames = new string[batchSize];

            for (var index = 0; index < batchSize; index++)
            {
                var parameterName = $"@p{index}";
                parameterNames[index] = parameterName;
                command.Parameters.AddWithValue(parameterName, normalizedIds[offset + index]);
            }

            command.CommandText = $"""
                SELECT
                    job_id, source_url, detail_url, title, company, duty_text, career_text, education_text, location_text,
                    game_category_text, employment_type_text, deadline_text, modified_text, modified_key, is_hidden,
                    first_seen_utc, last_seen_utc, last_changed_utc
                FROM job_postings
                WHERE source_url = @source_url
                  AND job_id IN ({string.Join(", ", parameterNames)})
                """;
            command.Parameters.AddWithValue("@source_url", sourceUrl);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var record = ReadRecord(reader);
                records[record.JobId] = record;
            }
        }

        return records;
    }

    public async Task<IReadOnlyCollection<string>> GetVisibleJobIdsAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT job_id
            FROM job_postings
            WHERE source_url = @source_url
              AND is_hidden = 0
            """;
        command.Parameters.AddWithValue("@source_url", sourceUrl);

        var ids = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            ids.Add(reader.GetString(0));
        }

        return ids;
    }

    public async Task UpsertAsync(
        IReadOnlyCollection<JobPostingRecord> records,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        if (records.Count == 0)
        {
            return;
        }

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        foreach (var record in records)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO job_postings (
                    job_id, source_url, detail_url, title, company, duty_text, career_text, education_text, location_text,
                    game_category_text, employment_type_text, deadline_text, modified_text, modified_key, is_hidden,
                    first_seen_utc, last_seen_utc, last_changed_utc
                ) VALUES (
                    @job_id, @source_url, @detail_url, @title, @company, @duty_text, @career_text, @education_text, @location_text,
                    @game_category_text, @employment_type_text, @deadline_text, @modified_text, @modified_key, @is_hidden,
                    @first_seen_utc, @last_seen_utc, @last_changed_utc
                )
                ON CONFLICT(source_url, job_id) DO UPDATE SET
                    source_url = excluded.source_url,
                    detail_url = excluded.detail_url,
                    title = excluded.title,
                    company = excluded.company,
                    duty_text = excluded.duty_text,
                    career_text = excluded.career_text,
                    education_text = excluded.education_text,
                    location_text = excluded.location_text,
                    game_category_text = excluded.game_category_text,
                    employment_type_text = excluded.employment_type_text,
                    deadline_text = excluded.deadline_text,
                    modified_text = excluded.modified_text,
                    modified_key = excluded.modified_key,
                    is_hidden = excluded.is_hidden,
                    first_seen_utc = excluded.first_seen_utc,
                    last_seen_utc = excluded.last_seen_utc,
                    last_changed_utc = excluded.last_changed_utc
                """;

            command.Parameters.AddWithValue("@job_id", record.JobId);
            command.Parameters.AddWithValue("@source_url", record.SourceUrl);
            command.Parameters.AddWithValue("@detail_url", record.DetailUrl);
            command.Parameters.AddWithValue("@title", record.Title);
            command.Parameters.AddWithValue("@company", record.Company);
            command.Parameters.AddWithValue("@duty_text", record.DutyText);
            command.Parameters.AddWithValue("@career_text", record.CareerText);
            command.Parameters.AddWithValue("@education_text", record.EducationText);
            command.Parameters.AddWithValue("@location_text", record.LocationText);
            command.Parameters.AddWithValue("@game_category_text", record.GameCategoryText);
            command.Parameters.AddWithValue("@employment_type_text", record.EmploymentTypeText);
            command.Parameters.AddWithValue("@deadline_text", record.DeadlineText);
            command.Parameters.AddWithValue("@modified_text", record.RegisteredText);
            command.Parameters.AddWithValue("@modified_key", string.Empty);
            command.Parameters.AddWithValue("@is_hidden", record.IsHidden ? 1 : 0);
            command.Parameters.AddWithValue("@first_seen_utc", record.FirstSeenUtc.ToString("O"));
            command.Parameters.AddWithValue("@last_seen_utc", record.LastSeenUtc.ToString("O"));
            command.Parameters.AddWithValue("@last_changed_utc", record.LastChangedUtc.ToString("O"));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task MarkHiddenAsync(
        string sourceUrl,
        IReadOnlyCollection<string> jobIds,
        DateTimeOffset hiddenAtUtc,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        if (jobIds.Count == 0)
        {
            return;
        }

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        foreach (var jobId in jobIds)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE job_postings
                SET
                    is_hidden = 1,
                    last_seen_utc = @last_seen_utc,
                    last_changed_utc = @last_changed_utc
                WHERE source_url = @source_url
                  AND job_id = @job_id
                  AND is_hidden = 0
                """;
            command.Parameters.AddWithValue("@last_seen_utc", hiddenAtUtc.ToString("O"));
            command.Parameters.AddWithValue("@last_changed_utc", hiddenAtUtc.ToString("O"));
            command.Parameters.AddWithValue("@source_url", sourceUrl);
            command.Parameters.AddWithValue("@job_id", jobId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static SqliteConnection CreateConnection()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = AppPaths.DatabaseFile
        };

        return new SqliteConnection(builder.ToString());
    }

    private static JobPostingRecord ReadRecord(SqliteDataReader reader)
    {
        return new JobPostingRecord
        {
            JobId = reader.GetString(0),
            SourceUrl = reader.GetString(1),
            DetailUrl = reader.GetString(2),
            Title = reader.GetString(3),
            Company = reader.GetString(4),
            DutyText = reader.GetString(5),
            CareerText = reader.GetString(6),
            EducationText = reader.GetString(7),
            LocationText = reader.GetString(8),
            GameCategoryText = reader.GetString(9),
            EmploymentTypeText = reader.GetString(10),
            DeadlineText = reader.GetString(11),
            RegisteredText = reader.GetString(12),
            IsHidden = reader.GetInt32(14) == 1,
            FirstSeenUtc = DateTimeOffset.Parse(reader.GetString(15)),
            LastSeenUtc = DateTimeOffset.Parse(reader.GetString(16)),
            LastChangedUtc = DateTimeOffset.Parse(reader.GetString(17))
        };
    }

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(connection, cancellationToken))
        {
            await CreateTableAsync(connection, cancellationToken);
            await EnsureIndexesAsync(connection, cancellationToken);
            return;
        }

        if (!await IsCompositeScopeSchemaAsync(connection, cancellationToken))
        {
            await MigrateLegacySchemaAsync(connection, cancellationToken);
        }

        await EnsureIndexesAsync(connection, cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(1)
            FROM sqlite_master
            WHERE type = 'table'
              AND name = @table_name
            """;
        command.Parameters.AddWithValue("@table_name", TableName);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> IsCompositeScopeSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({TableName});";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var sourcePkOrder = 0;
        var jobPkOrder = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            var columnName = reader.GetString(1);
            var pkOrder = reader.GetInt32(5);

            if (columnName.Equals("source_url", StringComparison.OrdinalIgnoreCase))
            {
                sourcePkOrder = pkOrder;
            }
            else if (columnName.Equals("job_id", StringComparison.OrdinalIgnoreCase))
            {
                jobPkOrder = pkOrder;
            }
        }

        return sourcePkOrder > 0 && jobPkOrder > 0;
    }

    private static async Task CreateTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS job_postings (
                source_url TEXT NOT NULL,
                job_id TEXT NOT NULL,
                detail_url TEXT NOT NULL,
                title TEXT NOT NULL,
                company TEXT NOT NULL,
                duty_text TEXT NOT NULL,
                career_text TEXT NOT NULL,
                education_text TEXT NOT NULL,
                location_text TEXT NOT NULL,
                game_category_text TEXT NOT NULL,
                employment_type_text TEXT NOT NULL,
                deadline_text TEXT NOT NULL,
                modified_text TEXT NOT NULL,
                modified_key TEXT NOT NULL,
                is_hidden INTEGER NOT NULL,
                first_seen_utc TEXT NOT NULL,
                last_seen_utc TEXT NOT NULL,
                last_changed_utc TEXT NOT NULL,
                PRIMARY KEY (source_url, job_id)
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureIndexesAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE INDEX IF NOT EXISTS idx_job_postings_source_visible
                ON job_postings(source_url, is_hidden);

            CREATE INDEX IF NOT EXISTS idx_job_postings_job_id
                ON job_postings(job_id);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task MigrateLegacySchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await ExecuteNonQueryAsync(connection, transaction, """
                ALTER TABLE job_postings RENAME TO job_postings_legacy;
                """, cancellationToken);

            await ExecuteNonQueryAsync(connection, transaction, """
                CREATE TABLE job_postings (
                    source_url TEXT NOT NULL,
                    job_id TEXT NOT NULL,
                    detail_url TEXT NOT NULL,
                    title TEXT NOT NULL,
                    company TEXT NOT NULL,
                    duty_text TEXT NOT NULL,
                    career_text TEXT NOT NULL,
                    education_text TEXT NOT NULL,
                    location_text TEXT NOT NULL,
                    game_category_text TEXT NOT NULL,
                    employment_type_text TEXT NOT NULL,
                    deadline_text TEXT NOT NULL,
                    modified_text TEXT NOT NULL,
                    modified_key TEXT NOT NULL,
                    is_hidden INTEGER NOT NULL,
                    first_seen_utc TEXT NOT NULL,
                    last_seen_utc TEXT NOT NULL,
                    last_changed_utc TEXT NOT NULL,
                    PRIMARY KEY (source_url, job_id)
                );
                """, cancellationToken);

            await ExecuteNonQueryAsync(connection, transaction, """
                INSERT INTO job_postings (
                    source_url, job_id, detail_url, title, company, duty_text, career_text, education_text, location_text,
                    game_category_text, employment_type_text, deadline_text, modified_text, modified_key, is_hidden,
                    first_seen_utc, last_seen_utc, last_changed_utc
                )
                SELECT
                    source_url, job_id, detail_url, title, company, duty_text, career_text, education_text, location_text,
                    game_category_text, employment_type_text, deadline_text, modified_text, modified_key, is_hidden,
                    first_seen_utc, last_seen_utc, last_changed_utc
                FROM job_postings_legacy;
                """, cancellationToken);

            await ExecuteNonQueryAsync(connection, transaction, """
                DROP TABLE job_postings_legacy;
                """, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
