package ca.bc.gov.tno;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

/**
 * ListHelper class, provides methods to help with lists.
 */
public class ListHelper {
  /**
   * Safely casts the collection into the specified list type.
   * 
   * @param <T>   Type of items in list.
   * @param clazz The class type of the items.
   * @param items The items to add to the list.
   * @return A new instance of a List.
   */
  public static <T> List<T> castList(Class<? extends T> clazz, Collection<?> items) {
    List<T> r = new ArrayList<T>(items.size());
    for (Object o : items)
      r.add(clazz.cast(o));
    return r;
  }
}
