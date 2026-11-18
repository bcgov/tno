
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IMediaTypeService : IBaseService<MediaType, int>
{
    IEnumerable<MediaType> FindAll();
}
