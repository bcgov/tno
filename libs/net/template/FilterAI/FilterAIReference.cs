namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIReference record, a reference value the filter settings refer to by id or code.
/// </summary>
/// <param name="Id">The primary key, when the settings refer to it by id.</param>
/// <param name="Code">The code, when the settings refer to it by code (tags).</param>
/// <param name="Name">The display name.</param>
/// <param name="Detail">Anything else that helps choose between values (i.e. an action's value type).</param>
public record FilterAIReference(int? Id, string? Code, string Name, string? Detail = null);
