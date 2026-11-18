using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Areas.Services.Controllers;
using TNO.API.Areas.Services.Models.ReportInstance;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using Xunit;

namespace TNO.Test.API;

public class ReportCollectionResponseTest
{
    [Fact]
    public async Task ContentUpdateReturnsVersionWithoutReturningStories()
    {
        var saved = new ReportInstance(7L, 21, null)
        {
            Version = 12,
        };
        saved.ContentManyToMany.Add(new ReportInstanceContent(7, 99, "section")
        {
            Content = new Content("test", "Headline", "Source", ContentType.PrintContent, 1, 1)
            { Id = 99, Body = new string('x', 100_000) },
        });
        var service = new Mock<IReportInstanceService>();
        service.Setup(s => s.UpdateAndSave(It.IsAny<ReportInstance>(), true)).Returns(saved);
        var controller = new ReportInstanceController(service.Object, Mock.Of<IUserService>(), Mock.Of<IKafkaMessenger>(),
            Options.Create(new KafkaHubConfig()), null!, Options.Create(new JsonSerializerOptions()), null!);
        var result = Assert.IsType<JsonResult>(await controller.UpdateAsync(new ReportInstanceModel { Id = 7, ReportId = 21, Version = 11 }, true));
        var model = Assert.IsType<ReportInstanceModel>(result.Value);
        Assert.Equal(7, model.Id);
        Assert.Equal(12, model.Version);
        Assert.Empty(model.Content);
        Assert.Single(saved.ContentManyToMany);
    }
}
