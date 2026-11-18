namespace TNO.Entities.Models;

public interface IPaged<T>
{
    /// <summary>
    /// get - List of items on page.
    /// </summary>
    public List<T> Items { get; }

    /// <summary>
    /// get/set - The page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// get/set - The number of items per page.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// get/set - The total number of items in the datasource.
    /// </summary>
    public long Total { get; set; }
}
