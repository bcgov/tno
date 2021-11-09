package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.DataSource;

/**
 * IDataSourceService interface, provides a way to interact with data sources.
 */
public interface IDataSourceService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of data source.
   */
  List<DataSource> findAll();

  /**
   * Find the data source for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the data source if it exists.
   */
  Optional<DataSource> findById(Integer key);

  /**
   * Find the data source for the specified code.
   * 
   * @param code The unique code.
   * @return A new instance of the data source if it exists.
   */
  Optional<DataSource> findByCode(String code);

  /**
   * Add a new data source to the data source.
   * 
   * @param entity The data source to add.
   * @return A new instance of the data source that was added.
   */
  DataSource add(DataSource entity);

  /**
   * Update the specified data source in the data source.
   * 
   * @param entity The data source to update.
   * @return A new instance of the data source that was updated.
   */
  DataSource update(DataSource entity);

  /**
   * Delete the specified data source from the data source.
   * 
   * @param entity The data source to delete.
   */
  void delete(DataSource entity);
}
