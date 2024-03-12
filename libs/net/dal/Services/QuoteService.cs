using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class QuoteService : BaseService<Quote, long>, IQuoteService {

    #region Constructors
    public QuoteService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<FileReferenceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }

    /// <summary>
    /// Attach a quote to a piece of content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="quote"></param>
    /// <returns></returns>
    public Quote Attach(Quote quote, bool commitTransaction = true)
    {
        if (quote.ContentId == 0) throw new ArgumentException("Parameter 'quote.ContentId' must be greater than zero.", nameof(quote));

        if (quote.Id == 0)
            this.Context.Add(quote);
        else
            this.Context.Update(quote);

        if (commitTransaction) this.Context.CommitTransaction();
        
        return quote;
    }

    /// <summary>
    /// Attach a quote to a piece of content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="quote"></param>
    /// <returns></returns>
    public IEnumerable<Quote> Attach(IEnumerable<Quote> quotes)
    {
        List<Quote> returnVal = new List<Quote>();
        foreach(var quote in quotes) {
            returnVal.Add(Attach(quote, false));
        }

        this.Context.CommitTransaction();
        
        return returnVal;
    }

    /// <summary>
    /// Find all file references for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public IEnumerable<Quote> FindByContentId(long contentId)
    {
        return this.Context.Quotes
            .Include(fr => fr.Content)
            .Where(fr => fr.ContentId == contentId).ToArray();
    }
    #endregion

}
