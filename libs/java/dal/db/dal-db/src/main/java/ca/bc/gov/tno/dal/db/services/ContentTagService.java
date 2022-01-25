package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentTag;
import ca.bc.gov.tno.dal.db.entities.ContentTagPK;
import ca.bc.gov.tno.dal.db.repositories.IContentTagRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentTagService;

/**
 * ContentTagService class, provides a concrete way to interact with
 * content
 * types in the database.
 */
@Service
public class ContentTagService implements IContentTagService {

  private final IContentTagRepository repository;

  /**
   * Creates a new instance of a ContentTagService object, initializes with
   * specified parameters.
   * 
   * @param repository The content tag repository.
   */
  @Autowired
  public ContentTagService(final IContentTagRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content tag.
   */
  @Override
  public List<ContentTag> findAll() {
    var ContentTags = (List<ContentTag>) repository.findAll();
    return ContentTags;
  }

  /**
   * Find the content tag for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content tag if it exists.
   */
  @Override
  public Optional<ContentTag> findById(ContentTagPK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content tag to the data source.
   * 
   * @param entity The content tag to add.
   * @return A new instance of the content tag that was added.
   */
  @Override
  public ContentTag add(ContentTag entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content tag in the data source.
   * 
   * @param entity The content tag to update.
   * @return A new instance of the content tag that was updated.
   */
  @Override
  public ContentTag update(ContentTag entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content tag from the data source.
   * 
   * @param entity The content tag to delete.
   */
  @Override
  public void delete(ContentTag entity) {
    repository.delete(entity);
  }

}
