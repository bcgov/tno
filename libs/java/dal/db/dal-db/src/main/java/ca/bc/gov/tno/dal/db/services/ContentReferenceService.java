package ca.bc.gov.tno.dal.db.services;

import java.time.Instant;
import java.util.Date;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;
import ca.bc.gov.tno.dal.db.repositories.IContentReferenceRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;

@Service
public class ContentReferenceService implements IContentReferenceService {

  @Autowired
  private IContentReferenceRepository repository;

  /**
   * Find all content references.
   */
  @Override
  public List<ContentReference> findAll() {
    var references = (List<ContentReference>) repository.findAll();
    return references;
  }

  /**
   * Find the content reference for the specified 'key'.
   */
  @Override
  public Optional<ContentReference> findById(ContentReferencePK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add the content reference.
   */
  @Override
  public ContentReference add(ContentReference reference) {
    // TODO: globally set this and get the current user.
    reference.setCreatedOn(Date.from(Instant.now()));
    reference.setCreatedById(UUID.fromString("00000000-0000-0000-0000-000000000000"));
    reference.setCreatedBy("unknown");
    var result = repository.save(reference);
    return result;
  }

  /**
   * Update the content reference.
   */
  @Override
  public ContentReference update(ContentReference reference) {
    // TODO: globally set this and get the current user.
    reference.setUpdatedOn(Date.from(Instant.now()));
    reference.setUpdatedById(UUID.fromString("00000000-0000-0000-0000-000000000000"));
    reference.setUpdatedBy("unknown");
    var result = repository.save(reference);
    return result;
  }

  /**
   * Delete the content reference.
   */
  @Override
  public void delete(ContentReference reference) {
    repository.delete(reference);
  }

}
