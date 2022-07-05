namespace TNO.Test.Entities;

using System.Diagnostics.CodeAnalysis;

[Trait("category", "unit")]
[Trait("group", "entities")]
[ExcludeFromCodeCoverage]
public class CacheTest
{
    [Fact]
    public void Constructor1()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var item = new TNO.Entities.Cache("test", value);

        // Assert
        Assert.Equal("test", item.Key);
        Assert.Equal(value.ToString(), item.Value);
        Assert.Equal("", item.Description);

        Assert.Equal("", item.CreatedBy);
        Assert.Equal(Guid.Empty, item.CreatedById);
        Assert.Equal(DateTime.MinValue, item.CreatedOn);
        Assert.Equal("", item.UpdatedBy);
        Assert.Equal(Guid.Empty, item.UpdatedById);
        Assert.Equal(DateTime.MinValue, item.UpdatedOn);
        Assert.Equal(0, item.Version);
    }

    [Fact]
    public void Constructor2()
    {
        // Arrange
        // Act
        var item = new TNO.Entities.Cache("test", "test");

        // Assert
        Assert.Equal("test", item.Key);
        Assert.Equal("test", item.Value);
        Assert.Equal("", item.Description);

        Assert.Equal("", item.CreatedBy);
        Assert.Equal(Guid.Empty, item.CreatedById);
        Assert.Equal(DateTime.MinValue, item.CreatedOn);
        Assert.Equal("", item.UpdatedBy);
        Assert.Equal(Guid.Empty, item.UpdatedById);
        Assert.Equal(DateTime.MinValue, item.UpdatedOn);
        Assert.Equal(0, item.Version);
    }
}
