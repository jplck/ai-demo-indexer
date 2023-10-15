public class BlobCloudEvent
{
    public required string Source { get; set; }
    public required string Subject { get; set; }
    public required string Type { get; set; }
    public DateTime Time { get; set; }
    public required string Id { get; set; }
    public required BlobCloudEventData Data { get; set; }
    public required string Specversion { get; set; }
}

public class BlobCloudEventData
{
    public required string Api { get; set; }
    public string? ClientRequestId { get; set; }
    public required string RequestId { get; set; }
    public string? ETag { get; set; }
    public required string ContentType { get; set; }
    public int ContentLength { get; set; }
    public required string BlobType { get; set; }
    public required string Url { get; set; }
    public required string Sequencer { get; set; }
    public required BlobCloudEventStorageDiagnostics StorageDiagnostics { get; set; }
}

public class BlobCloudEventStorageDiagnostics
{
    public required string BatchId { get; set; }
}