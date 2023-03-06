
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface ITopicService : IBaseService<Topic, int>
{
    IEnumerable<Topic> FindAll();
    IPaged<Topic> Find(TopicFilter filter);

}
