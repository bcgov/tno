
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IIngestStateService : IBaseService<IngestState, int>
{
    public IngestState AddOrUpdate(IngestState entity);
}
