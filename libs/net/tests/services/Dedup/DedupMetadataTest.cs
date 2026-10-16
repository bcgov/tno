using System.Text.Json;
using FluentAssertions;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.Services.Dedup;

namespace TNO.Test.Services.Dedup;

public class DedupMetadataTest
{
    private static readonly DateTime PublishedOn = new(2026, 10, 15, 21, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void WriteThenTryRead_RoundTripsFingerprint()
    {
        var entry = DedupEntry.Create("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel", Story.Original, PublishedOn, 0, new DedupOptions());
        var reference = new ContentReferenceModel() { Source = "VMS", Uid = "1", PublishedOn = PublishedOn };

        DedupMetadata.Write(reference.Metadata, entry);
        DedupMetadata.TryRead(reference, 2, out var result).Should().BeTrue();

        result!.Headline.Should().Be(entry.Headline);
        result.BodyHash.Should().Be(entry.BodyHash);
        result.PublishedOn.Should().Be(PublishedOn);
        result.Priority.Should().Be(2);
    }

    [Fact]
    public void TryRead_ReadsMetadataReturnedByTheApi()
    {
        var entry = DedupEntry.Create("VMS", "1", "Conservatives pledge to scrap new Massey Tunnel", Story.Original, PublishedOn, 0, new DedupOptions());
        var metadata = new Dictionary<string, object?>();
        DedupMetadata.Write(metadata, entry);

        // The API returns metadata values as JsonElements.
        var json = JsonSerializer.Serialize(new ContentReferenceModel() { Source = "VMS", Uid = "1", PublishedOn = PublishedOn, Metadata = metadata });
        var reference = JsonSerializer.Deserialize<ContentReferenceModel>(json)!;
        reference.Metadata[ContentReferenceMetaDataKeys.DedupHeadline].Should().BeOfType<JsonElement>();

        DedupMetadata.TryRead(reference, 0, out var result).Should().BeTrue();
        result!.Headline.Should().Be(entry.Headline);
        result.BodyHash.Should().Be(entry.BodyHash);
    }

    [Fact]
    public void Write_RemovesBodyHashWhenTextIsTooShort()
    {
        var metadata = new Dictionary<string, object?> { { ContentReferenceMetaDataKeys.DedupBodyHash, "0000000000000001" } };
        DedupMetadata.Write(metadata, DedupEntry.Create("VMS", "1", "Boat on fire near Peachland launch", "Too short.", PublishedOn, 0, new DedupOptions()));

        metadata.Should().ContainKey(ContentReferenceMetaDataKeys.DedupHeadline);
        metadata.Should().NotContainKey(ContentReferenceMetaDataKeys.DedupBodyHash);
    }

    [Fact]
    public void TryRead_ReferenceWithoutFingerprint()
    {
        var reference = new ContentReferenceModel() { Source = "VMS", Uid = "1", PublishedOn = PublishedOn };
        reference.Metadata[ContentReferenceMetaDataKeys.IngestSource] = "VMS";

        DedupMetadata.TryRead(reference, 0, out var result).Should().BeFalse();
        result.Should().BeNull();
    }
}
