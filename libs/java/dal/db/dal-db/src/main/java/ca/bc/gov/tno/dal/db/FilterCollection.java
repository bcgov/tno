package ca.bc.gov.tno.dal.db;

import java.util.ArrayList;
import java.util.List;

public class FilterCollection {
  private final List<Object> filters = new ArrayList<Object>();

  public FilterCollection() {
  }

  /**
   * @return List<Object> return the filters
   */
  public List<Object> getFilters() {
    return filters;
  }

  public <T> List<Object> addFilter(FilterParam<T> filter) {
    this.filters.add(filter);
    return this.filters;
  }

  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Boolean value) {
    return addFilter(new FilterParam<Boolean>(name, logicalOperator, value));
  }

  public List<Object> addFilter(String name, LogicalOperators logicalOperator, String value) {
    return addFilter(new FilterParam<String>(name, logicalOperator, value));
  }

  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Integer value) {
    return addFilter(new FilterParam<Integer>(name, logicalOperator, value));
  }

  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Long value) {
    return addFilter(new FilterParam<Long>(name, logicalOperator, value));
  }

  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Float value) {
    return addFilter(new FilterParam<Float>(name, logicalOperator, value));
  }

  public List<Object> addFilter(String name, LogicalOperators logicalOperator, Double value) {
    return addFilter(new FilterParam<Double>(name, logicalOperator, value));
  }

}
