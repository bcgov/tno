package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.PrintContent;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * IPrintContentService interface, provides a way to interact with print
 * contents.
 */
public interface IPrintContentService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of print content.
   */
  List<PrintContent> findAll();

  /**
   * Find a page of print content that match the query.
   * 
   * @param page     The page to pull print content from.
   * @param quantity Number of items to return in a page.
   * @param filter   An array of filter parameters.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of print content.
   */
  IPaged<PrintContent> find(int page, int quantity, FilterCollection filter, SortParam[] sort);

  /**
   * Find the print content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the print content if it exists.
   */
  Optional<PrintContent> findById(int key);

  /**
   * Find the print content for the specified primary key.
   * 
   * @param key   The primary key.
   * @param eager Whether to eager load related entities.
   * @return A new instance of the print content if it exists.
   */
  Optional<PrintContent> findById(int key, Boolean eager);

  /**
   * Add a new print content to the data source.
   * 
   * @param entity The print content to add.
   * @return A new instance of the print content that was added.
   */
  PrintContent add(PrintContent entity);

  /**
   * Update the specified print content in the data source.
   * 
   * @param entity The print content to update.
   * @return A new instance of the print content that was updated.
   */
  PrintContent update(PrintContent entity);

  /**
   * Delete the specified print content from the data source.
   * 
   * @param entity The print content to delete.
   */
  void delete(PrintContent entity);
}
