
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface ITopicService : IBaseService<Topic, int>
{
    IEnumerable<Topic> FindAll();
    IPaged<Topic> Find(TopicFilter filter);

}
