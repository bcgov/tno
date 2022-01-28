package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentTone;
import ca.bc.gov.tno.dal.db.entities.ContentTonePK;
import ca.bc.gov.tno.dal.db.repositories.IContentToneRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentToneService;

/**
 * ContentToneService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class ContentToneService implements IContentToneService {

  private final IContentToneRepository repository;

  /**
   * Creates a new instance of a ContentToneService object, initializes with
   * specified parameters.
   * 
   * @param repository The content tone repository.
   */
  @Autowired
  public ContentToneService(final IContentToneRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content tone.
   */
  @Override
  public List<ContentTone> findAll() {
    var ContentTones = (List<ContentTone>) repository.findAll();
    return ContentTones;
  }

  /**
   * Find the content tone for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content tone if it exists.
   */
  @Override
  public Optional<ContentTone> findById(ContentTonePK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content tone to the data source.
   * 
   * @param entity The content tone to add.
   * @return A new instance of the content tone that was added.
   */
  @Override
  public ContentTone add(ContentTone entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content tone in the data source.
   * 
   * @param entity The content tone to update.
   * @return A new instance of the content tone that was updated.
   */
  @Override
  public ContentTone update(ContentTone entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content tone from the data source.
   * 
   * @param entity The content tone to delete.
   */
  @Override
  public void delete(ContentTone entity) {
    repository.delete(entity);
  }

}
