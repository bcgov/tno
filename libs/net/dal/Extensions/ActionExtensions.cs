using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;

namespace TNO.DAL.Extensions;

/// <summary>
/// ActionExtensions static class, provides extension methods for actions.
/// </summary>
public static class ActionExtensions
{
    /// <summary>
    /// Update the context entity state.
    /// </summary>
    /// <param name="updated"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Entities.Action AddToContext(this Entities.Action updated, TNOContext context)
    {
        updated.ContentTypes.SetEntityState(context);
        return updated;
    }

    /// <summary>
    /// Update the data source in context.
    /// Provides a way to handle child collection changes.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="original"></param>
    /// <param name="updated"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    public static TNOContext UpdateContext(this TNOContext context, Entities.Action original, Entities.Action updated)
    {
        var oContentTypes = context.ContentTypeActions.Where(m => m.ActionId == updated.Id).ToArray();
        oContentTypes.Except(updated.ContentTypes).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.ContentTypes.ForEach(a =>
        {
            var current = oContentTypes.FirstOrDefault(o => o.ContentType == a.ContentType);
            if (current == null)
                original.ContentTypes.Add(a);
            else
                context.Entry(current).State = EntityState.Unchanged;
        });

        context.Entry(original).CurrentValues.SetValues(updated);
        context.ResetVersion(original);
        return context;
    }
}
