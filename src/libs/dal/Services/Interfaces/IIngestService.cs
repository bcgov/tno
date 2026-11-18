
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IIngestService : IBaseService<Ingest, int>
{
    IEnumerable<Ingest> FindAll(bool includeConnection = false);
    IPaged<Ingest> Find(IngestFilter filter);
    IEnumerable<Ingest> FindByTopic(string topic, bool includeConnection = false);
    IEnumerable<Ingest> FindByIngestType(string ingestTypeName, bool includeConnection = false);
    Ingest UpdateAndSave(Ingest entity, bool updateChildren = false);
}
