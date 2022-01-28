package ca.bc.gov.tno.models.interfaces;

import java.util.List;

/**
 * IPaged interface, provides a way to page through content.
 */
public interface IPaged<T> {
  /**
   * The items in the page.
   * 
   * @return A List of items.
   */
  List<T> getItems();

  /**
   * The page number.
   * 
   * @return The page number.
   */
  int getPage();

  /**
   * The number of items per page.
   * 
   * @return The number of items per page.
   */
  int getQuantity();

  /**
   * The total number of items in the datasource.
   * 
   * @return The total number of items in the datasource.
   */
  long getTotal();
}
