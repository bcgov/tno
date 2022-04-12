
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IContentTypeService : IBaseService<ContentType, int>
{
    IEnumerable<ContentType> FindAll();
}
