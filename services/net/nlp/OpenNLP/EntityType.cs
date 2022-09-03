namespace TNO.Services.NLP.OpenNLP;

/// <summary>
/// EntityType enum, provides options for extracting entities from text.
/// </summary>
public enum EntityType
{
    Date = 0,
    Location,
    Money,
    Organization,
    Person,
    Time
}
