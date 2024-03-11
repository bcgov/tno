using TNO.Entities;
namespace TNO.DAL.Services;

public interface IQuoteService : IBaseService<Quote, long> {
    IEnumerable<Quote> FindByContentId(long contentId);
    Quote Attach(Content content, Quote quote, bool commitTransaction = true);
    IEnumerable<Quote> Attach(Content content, IEnumerable<Quote> quotes);
}
