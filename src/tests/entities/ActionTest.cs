namespace TNO.Test.Entities;

using System.Diagnostics.CodeAnalysis;
using TNO.Entities;

[Trait("category", "unit")]
[Trait("group", "entities")]
[ExcludeFromCodeCoverage]
public class ActionTest
{
    [Fact]
    public void Constructor1()
    {
        // Arrange
        // Act
        var item = new TNO.Entities.Action("test", ValueType.Numeric);

        // Assert
        Assert.Equal(0, item.Id);
        Assert.Equal("test", item.Name);
        Assert.Equal("", item.Description);
        Assert.True(item.IsEnabled);
        Assert.Equal(0, item.SortOrder);

        Assert.Equal("", item.CreatedBy);
        Assert.Equal(DateTime.MinValue, item.CreatedOn);
        Assert.Equal("", item.UpdatedBy);
        Assert.Equal(DateTime.MinValue, item.UpdatedOn);
        Assert.Equal(0, item.Version);

        Assert.Equal(ValueType.Numeric, item.ValueType);
        Assert.Equal("", item.ValueLabel);
        Assert.Equal("", item.DefaultValue);
        Assert.Empty(item.Contents);
        Assert.Empty(item.ContentsManyToMany);
        Assert.Empty(item.ContentTypes);
    }

    [Fact]
    public void Constructor2()
    {
        // Arrange
        // Act
        var item = new TNO.Entities.Action("test", ValueType.String, "test");

        // Assert
        Assert.Equal(0, item.Id);
        Assert.Equal("test", item.Name);
        Assert.Equal("", item.Description);
        Assert.True(item.IsEnabled);
        Assert.Equal(0, item.SortOrder);

        Assert.Equal("", item.CreatedBy);
        Assert.Equal(DateTime.MinValue, item.CreatedOn);
        Assert.Equal("", item.UpdatedBy);
        Assert.Equal(DateTime.MinValue, item.UpdatedOn);
        Assert.Equal(0, item.Version);

        Assert.Equal(ValueType.String, item.ValueType);
        Assert.Equal("test", item.ValueLabel);
        Assert.Equal("", item.DefaultValue);
        Assert.Empty(item.Contents);
        Assert.Empty(item.ContentsManyToMany);
        Assert.Empty(item.ContentTypes);
    }
}
