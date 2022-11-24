using Microsoft.AspNetCore.Http;
using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.Test.DAL.Models;

public class ContentFileReferenceTest
{
    [Fact]
    public void Constructor1()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var text = "Hello World from a Fake File";
        var fileName = "test.pdf";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;

        var content = new Content("uid", "headline", "source", 0, 0, 0)
        {
            Id = 1
        };
        var file = new FormFile(stream, 0, stream.Length, "id_from_form", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "plain/text"
        };

        // Act
        var item = new ContentFileReference(content, file);

        // Assert
        Assert.Equal(0, item.Id);
        Assert.Equal(content.Id, item.ContentId);
        Assert.Equal(content.Version, item.Version);
        Assert.Equal(content, item.Content);
        Assert.Equal(file, item.File);
        Assert.Equal("source/source-1.pdf", item.Path);
        Assert.Equal(file.FileName, item.FileName);
        Assert.Equal("plain/text", item.ContentType);
        Assert.Equal(file.Length, item.Size);
        Assert.Equal(0, item.RunningTime);

        Assert.Equal(content.CreatedBy, item.CreatedBy);
        Assert.True(item.CreatedOn >= now);
        Assert.Equal(content.UpdatedBy, item.UpdatedBy);
        Assert.True(item.UpdatedOn >= now);
    }
}
