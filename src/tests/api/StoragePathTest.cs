using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Areas.Editor.Controllers;
using TNO.API.Config;
using TNO.Core.Storage;
using TNO.Core.Storage.Configuration;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.API.Areas.Editor.Models.Storage;
using TNO.API.Helpers;
using Xunit;

namespace TNO.Test.API;

public class StoragePathTest : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("tno-storage-test-").FullName;
    private string Root => Path.Combine(_directory, "storage");

    public StoragePathTest() => Directory.CreateDirectory(Root);

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("nested/../../outside.txt")]
    [InlineData("../storage-other/outside.txt")]
    public void TraversalCannotReadMetadata(string path)
    {
        Assert.Throws<ArgumentException>(() => new ItemModel(Root, path, true));
    }

    [Fact]
    public void AbsolutePathOutsideRootIsRejected()
    {
        Assert.Throws<ArgumentException>(() => StoragePath.Resolve(Root, Path.Combine(_directory, "outside.txt")));
    }

    [Fact]
    public void NormalFilesAndNewNestedPathsStayInRoot()
    {
        var file = Path.Combine(Root, "news.txt");
        File.WriteAllText(file, "news");
        var model = new ItemModel(Root, "news.txt", true);
        Assert.Equal("news.txt", model.Name);
        Assert.Equal(4, model.Size);
        Assert.False(model.IsDirectory);
        Assert.Equal(Path.Combine(Root, "new", "nested", "news.txt"), StoragePath.Resolve(Root, "new/nested/news.txt"));
        Assert.Equal(Root, StoragePath.Resolve(Root, ""));
    }

    [Fact]
    public void SymbolicLinksCannotEscapeRoot()
    {
        var target = Path.Combine(_directory, "outside");
        Directory.CreateDirectory(target);
        Directory.CreateSymbolicLink(Path.Combine(Root, "link"), target);
        Assert.Throws<ArgumentException>(() => StoragePath.Resolve(Root, "link/new.txt"));
        File.CreateSymbolicLink(Path.Combine(Root, "dangling"), Path.Combine(target, "missing.txt"));
        Assert.Throws<ArgumentException>(() => StoragePath.Resolve(Root, "dangling"));
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("..\\outside.txt")]
    [InlineData("/outside.txt")]
    [InlineData("C:\\outside.txt")]
    [InlineData("nested/file.txt")]
    [InlineData("")]
    public void UploadAndClipNamesCannotContainPaths(string name)
    {
        Assert.ThrowsAny<ArgumentException>(() => StoragePath.FileName(name));
    }

    private StorageController Controller() => new(
        Mock.Of<IConnectionHelper>(), Options.Create(new StorageOptions { UploadPath = Root }),
        Options.Create(new ApiOptions()), Mock.Of<IFileReferenceService>(),
        Mock.Of<ILogger<StorageController>>(), Options.Create(new S3Options()), Mock.Of<IS3StorageService>());

    [Theory]
    [InlineData("../", "outside.txt")]
    [InlineData("", "../outside.txt")]
    [InlineData("", "..\\outside.txt")]
    public async Task UploadRejectsTraversalBeforeCreatingAnyFile(string path, string name)
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var file = new FormFile(stream, 0, stream.Length, "file", name);
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Controller().Upload(null, path, [file]));
        Assert.Empty(Directory.GetFiles(_directory, "*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task UploadCreatesFileWithinNewNestedDirectory()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var file = new FormFile(stream, 0, stream.Length, "file", "news.txt");
        await Controller().Upload(null, "new/nested", [file]);
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(Path.Combine(Root, "new", "nested", "news.txt")));
    }

    [Fact]
    public void MoveRejectsOutsideDestinationAndPreservesSource()
    {
        File.WriteAllText(Path.Combine(Root, "news.txt"), "news");
        Assert.Throws<ArgumentException>(() => Controller().Move(null, "news.txt", "../outside.txt"));
        Assert.True(File.Exists(Path.Combine(Root, "news.txt")));
        Assert.False(File.Exists(Path.Combine(_directory, "outside.txt")));
    }

    public void Dispose() => Directory.Delete(_directory, true);
}
