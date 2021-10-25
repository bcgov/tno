package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
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
  public ContentReference add(ContentReference entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the content reference.
   */
  @Override
  public ContentReference update(ContentReference entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
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
