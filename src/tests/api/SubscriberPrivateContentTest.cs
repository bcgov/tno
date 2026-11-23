using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Areas.Subscriber.Controllers;
using TNO.API.Areas.Subscriber.Models.Content;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.DAL;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using Xunit;

namespace TNO.Test.API;

public class SubscriberPrivateContentTest
{
    [Fact]
    public void CreatingPrivateContentQueuesIndexingBeforeSavingWithTheCurrentOwner()
    {
        var contents = new Mock<IContentService>(MockBehavior.Strict);
        var impersonation = new Mock<IImpersonationHelper>();
        impersonation.Setup(x => x.GetCurrentUser()).Returns(new User("owner", "owner@example.com") { Id = 42 });
        var sequence = new MockSequence();
        contents.InSequence(sequence).Setup(x => x.RequestIndex(
            It.Is<Content>(c => c.IsPrivate && c.OwnerId == 42 && c.Status == ContentStatus.Published),
            null, 42, TNOContext.IndexReasonLifecycle));
        contents.InSequence(sequence).Setup(x => x.AddAndSave(It.IsAny<Content>()))
            .Returns((Content content) => { content.Id = 99; return content; });
        var controller = new ContentController(contents.Object, null!, null!, impersonation.Object, null!,
            Options.Create(new KafkaHubConfig()), Options.Create(new StorageOptions()), Options.Create(new ElasticOptions()),
            null!, Options.Create(new KafkaOptions()), Options.Create(new JsonSerializerOptions()), null!, NullLogger<ContentController>.Instance);

        var result = Assert.IsType<CreatedAtActionResult>(controller.Add(new ContentModel
        {
            Headline = "My private article",
            IsPrivate = true,
            Status = ContentStatus.Published,
            OwnerId = 123,
            Uid = "private-story",
            OtherSource = "Manual source",
            LicenseId = 1,
            MediaTypeId = 1,
        }));

        var saved = Assert.IsType<ContentModel>(result.Value);
        Assert.Equal(42, saved.OwnerId);
        Assert.Equal(99, saved.Id);
        contents.VerifyAll();
    }
}
