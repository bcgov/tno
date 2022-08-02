
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;
namespace TNO.DAL.Services;

public interface ITagService : IBaseService<Tag, string>
{
    IEnumerable<Tag> FindAll();
    IPaged<Tag> Find(TagFilter filter);
}
