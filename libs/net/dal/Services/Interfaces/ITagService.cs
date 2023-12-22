
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;
namespace TNO.DAL.Services;

public interface ITagService : IBaseService<Tag, int>
{
    IEnumerable<Tag> FindAll();

    IEnumerable<Tag> FindAllEnabled();
    IPaged<Tag> Find(TagFilter filter);
}
