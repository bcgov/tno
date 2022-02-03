package ca.bc.gov.tno.dal.db.models;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

/**
 * FilterCollection class, provides a way to group related filter parameters.
 */
public class FilterCollection {
  private final List<Object> filters = new ArrayList<Object>();

  /**
   * Creates a new instance of a FilterCollection object.
   */
  public FilterCollection() {
  }

  /**
   * @return List{Object} return the filters
   */
  public List<Object> getFilters() {
    return filters;
  }

  /**
   * Add a filter to the collection.
   * 
   * @param <T>    The value type.
   * @param filter The filter parameter.
   * @return The list of filter parameters.
   */
  public <T> List<Object> addFilter(FilterParam<T> filter) {
    this.filters.add(filter);
    return this.filters;
  }

  /**
   * Add a filter to the collection.
   * 
   * @param name            The property name to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value of the property.
   * @return The list of filter parameters.
   */
  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Boolean value) {
    return addFilter(new FilterParam<Boolean>(Boolean.class, name, logicalOperator, value));
  }

  /**
   * Add a filter to the collection.
   * 
   * @param name            The property name to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value of the property.
   * @return The list of filter parameters.
   */
  public List<Object> addFilter(String name, LogicalOperators logicalOperator, String value) {
    return addFilter(new FilterParam<String>(String.class, name, logicalOperator, value));
  }

  /**
   * Add a filter to the collection.
   * 
   * @param name            The property name to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value of the property.
   * @return The list of filter parameters.
   */
  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Integer value) {
    return addFilter(new FilterParam<Integer>(Integer.class, name, logicalOperator, value));
  }

  /**
   * Add a filter to the collection.
   * 
   * @param name            The property name to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value of the property.
   * @return The list of filter parameters.
   */
  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Long value) {
    return addFilter(new FilterParam<Long>(Long.class, name, logicalOperator, value));
  }

  /**
   * Add a filter to the collection.
   * 
   * @param name            The property name to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value of the property.
   * @return The list of filter parameters.
   */
  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Float value) {
    return addFilter(new FilterParam<Float>(Float.class, name, logicalOperator, value));
  }

  /**
   * Add a filter to the collection.
   * 
   * @param name            The property name to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value of the property.
   * @return The list of filter parameters.
   */
  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Double value) {
    return addFilter(new FilterParam<Double>(Double.class, name, logicalOperator, value));
  }

  /**
   * Add a filter to the collection.
   * 
   * @param name            The property name to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value of the property.
   * @return The list of filter parameters.
   */
  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Date value) {
    return addFilter(new FilterParam<Date>(Date.class, name, logicalOperator, value));
  }

}
