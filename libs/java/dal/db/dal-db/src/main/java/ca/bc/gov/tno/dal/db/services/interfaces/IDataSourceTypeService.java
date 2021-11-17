package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.DataSourceType;

/**
 * IDataSourceTypeService interface, provides a way to interact with data source
 * types.
 */
public interface IDataSourceTypeService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of data source type.
   */
  List<DataSourceType> findAll();

  /**
   * Find the data source type for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the data source type if it exists.
   */
  Optional<DataSourceType> findById(Integer key);

  /**
   * Add a new data source type to the data source.
   * 
   * @param entity The data source type to add.
   * @return A new instance of the data source type that was added.
   */
  DataSourceType add(DataSourceType entity);

  /**
   * Update the specified data source type in the data source.
   * 
   * @param entity The data source type to update.
   * @return A new instance of the data source type that was updated.
   */
  DataSourceType update(DataSourceType entity);

  /**
   * Delete the specified data source type from the data source.
   * 
   * @param entity The data source type to delete.
   */
  void delete(DataSourceType entity);
}
