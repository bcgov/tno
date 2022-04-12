
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ITagService : IBaseService<Tag, string>
{
    IEnumerable<Tag> FindAll();
}
