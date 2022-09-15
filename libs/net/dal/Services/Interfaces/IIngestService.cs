
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IIngestService : IBaseService<Ingest, int>
{
    IEnumerable<Ingest> FindAll(bool includeConnection = false);
    IPaged<Ingest> Find(IngestFilter filter);
    IEnumerable<Ingest> FindByTopic(string topic, bool includeConnection = false);
    IEnumerable<Ingest> FindByMediaType(string mediaTypeName, bool includeConnection = false);
    Ingest Update(Ingest entity, bool updateChildren = false);
}
