
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;
namespace TNO.DAL.Services;

public interface ITagService : IBaseService<Tag, int>
{
    IEnumerable<Tag> FindAll();
    IPaged<Tag> Find(TagFilter filter);
}
