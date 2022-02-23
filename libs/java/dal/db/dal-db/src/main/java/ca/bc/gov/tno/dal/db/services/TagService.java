package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Tag;
import ca.bc.gov.tno.dal.db.repositories.interfaces.ITagRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ITagService;

/**
 * TagService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class TagService implements ITagService {

  private final ITagRepository repository;

  /**
   * Creates a new instance of a TagService object, initializes with
   * specified parameters.
   * 
   * @param repository The tag repository.
   */
  @Autowired
  public TagService(final ITagRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of tag.
   */
  @Override
  public List<Tag> findAll() {
    var result = (List<Tag>) repository.findAll();
    return result;
  }

  /**
   * Find the tag for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the tag if it exists.
   */
  @Override
  public Optional<Tag> findById(String key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Add a new tag to the data source.
   * 
   * @param entity The tag to add.
   * @return A new instance of the tag that was added.
   */
  @Override
  public Tag add(Tag entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified tag in the data source.
   * 
   * @param entity The tag to update.
   * @return A new instance of the tag that was updated.
   */
  @Override
  public Tag update(Tag entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified tag from the data source.
   * 
   * @param entity The tag to delete.
   */
  @Override
  public void delete(Tag entity) {
    repository.delete(entity);
  }

}
