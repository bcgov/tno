package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.dal.db.repositories.IContentRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentService;

/**
 * ContentService class, provides a concrete way to interact with content
 * in the database.
 */
@Service
public class ContentService implements IContentService {

  private final IContentRepository repository;

  /**
   * Creates a new instance of a ContentService object, initializes with
   * specified parameters.
   * 
   * @param repository The content repository.
   */
  @Autowired
  public ContentService(final IContentRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content.
   */
  @Override
  public List<Content> findAll() {
    var Contents = (List<Content>) repository.findAll();
    return Contents;
  }

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  @Override
  public Optional<Content> findById(int key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content to the data source.
   * 
   * @param entity The content to add.
   * @return A new instance of the content that was added.
   */
  @Override
  public Content add(Content entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content in the data source.
   * 
   * @param entity The content to update.
   * @return A new instance of the content that was updated.
   */
  @Override
  public Content update(Content entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content from the data source.
   * 
   * @param entity The content to delete.
   */
  @Override
  public void delete(Content entity) {
    repository.delete(entity);
  }

}
