using TNO.Entities;

namespace TNO.DAL.Extensions;

public static class TonePoolExtensions
{
    
    /// <summary>
    /// Update the context entity state so that it and related entities are added to the database.
    /// </summary>
    /// <param name="updated"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TonePool AddToContext(this TonePool updated, TNOContext context)
    {
        updated.ContentsManyToMany.SetEntityState(context);
        return updated;
    }
}