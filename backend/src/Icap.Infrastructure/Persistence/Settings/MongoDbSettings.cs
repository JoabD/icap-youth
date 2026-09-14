namespace Icap.Infrastructure.Persistence.Settings;

/// <summary>
/// Se enlaza a la sección "MongoDbSettings" de appsettings.json.
/// Nombres de colecciones configurables para no hardcodearlos en los repos.
/// </summary>
public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";

    public string ConnectionString { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
    public string UsersCollectionName { get; set; } = "Users";
    public string ReceiptsCollectionName { get; set; } = "Receipts";
    public string CountersCollectionName { get; set; } = "Counters";
}
