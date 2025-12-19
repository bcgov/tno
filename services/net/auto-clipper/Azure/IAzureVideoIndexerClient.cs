using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TNO.Services.AutoClipper.Azure;

public interface IAzureVideoIndexerClient
{
    Task<IReadOnlyList<TimestampedTranscript>> GenerateTranscriptAsync(Stream stream, string fileName, string language, CancellationToken cancellationToken = default);
}
