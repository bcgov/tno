package ca.bc.gov.tno.models;

import java.util.List;

import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * Paged class, provides the ability to page through items of the specified
 * type.
 */
public class Paged<T> implements IPaged<T> {
  private List<T> items;

  private int page;

  private int quantity;

  private long total;

  /**
   * Creates a new instance of a Paged object, initializes with specified
   * arguments.
   * 
   * @param items    List of items in the page.
   * @param page     The page number.
   * @param quantity The quantity requested.
   * @param total    The total number of items in the datasource.
   */
  public Paged(final List<T> items, final int page, final int quantity, final long total) {
    if (items == null)
      throw new IllegalArgumentException("Argument 'items' cannot be null");

    this.items = items;
    this.page = page < 1 ? 1 : page;
    this.quantity = quantity < 1 ? 1 : quantity;
    this.total = total < 0 ? 0 : total;
  }

  /**
   * @return List{T} return the items
   */
  public List<T> getItems() {
    return items;
  }

  /**
   * @return int return the page
   */
  public int getPage() {
    return page;
  }

  /**
   * @return int return the quantity
   */
  public int getQuantity() {
    return quantity;
  }

  /**
   * @return long return the total
   */
  public long getTotal() {
    return total;
  }

}
