package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentLink;
import ca.bc.gov.tno.dal.db.entities.ContentLinkPK;

/**
 * IContentLinkService interface, provides a way to interact with content
 * types.
 */
public interface IContentLinkService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content link.
   */
  List<ContentLink> findAll();

  /**
   * Find the content link for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content link if it exists.
   */
  Optional<ContentLink> findById(ContentLinkPK key);

  /**
   * Find all content link for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content link if it exists.
   */
  List<ContentLink> findByContentId(int contentId);

  /**
   * Add a new content link to the content.
   * 
   * @param entity The content link to add.
   * @return A new instance of the content link that was added.
   */
  ContentLink add(ContentLink entity);

  /**
   * Add a new content link to the data source.
   * 
   * @param entities An array of content link to add.
   * @return A new instance of the content link that was added.
   */
  Iterable<ContentLink> add(Iterable<ContentLink> entities);

  /**
   * Update the specified content link in the content.
   * 
   * @param entity The content link to update.
   * @return A new instance of the content link that was updated.
   */
  ContentLink update(ContentLink entity);

  /**
   * Update the specified content link in the data source.
   * 
   * @param entities An array of content link to update.
   * @return A new instance of the content link that was updated.
   */
  Iterable<ContentLink> update(Iterable<ContentLink> entities);

  /**
   * Delete the specified content link from the content.
   * 
   * @param entity The content link to delete.
   */
  void delete(ContentLink entity);

  /**
   * Delete the specified content link from the data source.
   * 
   * @param entities An array of content link to delete.
   */
  void delete(Iterable<ContentLink> entities);
}
