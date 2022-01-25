package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentLink;
import ca.bc.gov.tno.dal.db.entities.ContentLinkPK;
import ca.bc.gov.tno.dal.db.repositories.IContentLinkRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentLinkService;

/**
 * ContentLinkService class, provides a concrete way to interact with
 * content
 * types in the database.
 */
@Service
public class ContentLinkService implements IContentLinkService {

  private final IContentLinkRepository repository;

  /**
   * Creates a new instance of a ContentLinkService object, initializes with
   * specified parameters.
   * 
   * @param repository The content link repository.
   */
  @Autowired
  public ContentLinkService(final IContentLinkRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content link.
   */
  @Override
  public List<ContentLink> findAll() {
    var ContentLinks = (List<ContentLink>) repository.findAll();
    return ContentLinks;
  }

  /**
   * Find the content link for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content link if it exists.
   */
  @Override
  public Optional<ContentLink> findById(ContentLinkPK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content link to the data source.
   * 
   * @param entity The content link to add.
   * @return A new instance of the content link that was added.
   */
  @Override
  public ContentLink add(ContentLink entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content link in the data source.
   * 
   * @param entity The content link to update.
   * @return A new instance of the content link that was updated.
   */
  @Override
  public ContentLink update(ContentLink entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content link from the data source.
   * 
   * @param entity The content link to delete.
   */
  @Override
  public void delete(ContentLink entity) {
    repository.delete(entity);
  }

}
