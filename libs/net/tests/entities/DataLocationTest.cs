namespace TNO.Test.Entities;

using System.Diagnostics.CodeAnalysis;
using TNO.Entities;

[Trait("category", "unit")]
[Trait("group", "entities")]
[ExcludeFromCodeCoverage]
public class DataLocationTest
{
    [Fact]
    public void Constructor1()
    {
        // Arrange
        // Act
        var item = new TNO.Entities.DataLocation("test");

        // Assert
        Assert.Equal(0, item.Id);
        Assert.Equal("test", item.Name);
        Assert.Equal("", item.Description);
        Assert.True(item.IsEnabled);
        Assert.Equal(0, item.SortOrder);

        Assert.Equal(DataLocationType.LocalVolume, item.LocationType);
        Assert.Equal("{}", item.Connection);

        Assert.Equal("", item.CreatedBy);
        Assert.Equal(Guid.Empty, item.CreatedById);
        Assert.Equal(DateTime.MinValue, item.CreatedOn);
        Assert.Equal("", item.UpdatedBy);
        Assert.Equal(Guid.Empty, item.UpdatedById);
        Assert.Equal(DateTime.MinValue, item.UpdatedOn);
        Assert.Equal(0, item.Version);

        Assert.Empty(item.DataSources);
    }
}
