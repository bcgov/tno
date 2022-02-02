package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.FilterCollection;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * IContentService interface, provides a way to interact with contents.
 */
public interface IContentService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content.
   */
  List<Content> findAll();

  /**
   * Find a page of content that match the query.
   * 
   * @param page     The page to pull content from.
   * @param quantity Number of items to return in a page.
   * @param filter   An array of filter parameters.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of content.
   */
  IPaged<Content> find(int page, int quantity, FilterCollection filter, SortParam[] sort);

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  Optional<Content> findById(int key);

  /**
   * Find the content for the specified primary key.
   * 
   * @param key   The primary key.
   * @param eager Whether to eager load related entities.
   * @return A new instance of the content if it exists.
   */
  Optional<Content> findById(int key, Boolean eager);

  /**
   * Add a new content to the data source.
   * 
   * @param entity The content to add.
   * @return A new instance of the content that was added.
   */
  Content add(Content entity);

  /**
   * Update the specified content in the data source.
   * 
   * @param entity The content to update.
   * @return A new instance of the content that was updated.
   */
  Content update(Content entity);

  /**
   * Delete the specified content from the data source.
   * 
   * @param entity The content to delete.
   */
  void delete(Content entity);
}
