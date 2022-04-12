using TNO.Entities;

namespace TNO.Entities.Models;

public class Paged<T> : IPaged<T>
{
    #region Properties
    public List<T> Items { get; } = new List<T>();

    public int Page { get; set; }
    public int Quantity { get; set; }
    public long Total { get; set; }
    #endregion

    #region Constructors
    public Paged() { }

    public Paged(IEnumerable<T> items, int page = 1, int quantity = 1, long total = 0)
    {
        this.Items.AddRange(items);
        this.Page = page;
        this.Quantity = quantity;
        this.Total = total == 0 && this.Items.Count > 0 ? items.Count() : total;
    }
    #endregion
}
