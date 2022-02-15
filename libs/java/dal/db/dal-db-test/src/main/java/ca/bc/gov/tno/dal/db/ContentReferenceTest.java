package ca.bc.gov.tno.dal.db;

import java.util.Date;
import java.util.Optional;
import java.util.UUID;

import org.hibernate.exception.ConstraintViolationException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.dao.DataIntegrityViolationException;
import org.springframework.orm.ObjectOptimisticLockingFailureException;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;

@Component
public class ContentReferenceTest {
  IContentReferenceService contentReferenceService;

  @Autowired
  public ContentReferenceTest(final IContentReferenceService contentReferenceService) {
    this.contentReferenceService = contentReferenceService;
  }

  public void Run() {
    var result = Add();
    result = Update(result);
    FindById(result);
    Delete(result);
  }

  public ContentReference Add() {
    var reference = new ContentReference("NTLP", "uid-01", "topic");
    var added = contentReferenceService.add(reference);
    if (added.getCreatedOn() == null || added.getUpdatedOn() == null)
      throw new IllegalStateException("Audit dates were not set");
    if (added.getCreatedOn().compareTo(added.getUpdatedOn()) != 0)
      throw new IllegalStateException("Audit dates must be the same");

    try {
      // Test for duplication.
      var duplicate = new ContentReference("NTLP", "uid-01", "topic");
      contentReferenceService.add(duplicate);
    } catch (DataIntegrityViolationException e) {
      var cause = (ConstraintViolationException) e.getCause();
      if (!cause.getConstraintName().equals("pk_content_reference"))
        throw new IllegalStateException("Constraint missing from table");
      System.out.println("Duplication Check Successful: " + reference.getSource());
    }

    return added;
  }

  public ContentReference Update(ContentReference entity) {
    entity.setOffset(1);
    entity.setCreatedBy("illegal");
    entity.setCreatedById(UUID.randomUUID());

    var updated = contentReferenceService.update(entity);
    if (entity.getCreatedOn().compareTo(updated.getCreatedOn()) != 0)
      throw new IllegalStateException("Audit createdOn must not change");
    if (entity.getCreatedById() != updated.getCreatedById())
      throw new IllegalStateException("Audit createdById must not change");
    if (entity.getCreatedBy() != updated.getCreatedBy())
      throw new IllegalStateException("Audit createdBy must not change");
    if (entity.getUpdatedOn().compareTo(updated.getUpdatedOn()) >= 0)
      throw new IllegalStateException("Audit updatedOn must be after prior timestamp");

    var updatedOn = updated.getUpdatedOn();
    try {
      // Test for optimistic concurrency.
      updated.setUpdatedOn(new Date(System.currentTimeMillis()));
      contentReferenceService.update(updated);
      updated.setUpdatedOn(updatedOn);
      throw new IllegalStateException("Concurrency Failed");
    } catch (ObjectOptimisticLockingFailureException e) {
      // Success
    } catch (Exception e) {
      throw new IllegalStateException("Concurrency Failed");
    } finally {
      updated.setUpdatedOn(updatedOn);
    }

    return updated;
  }

  public Optional<ContentReference> FindById(ContentReference entity) {

    var result = contentReferenceService.findById(new ContentReferencePK(entity.getSource(), entity.getUid()));
    if (result.isEmpty())
      throw new IllegalStateException();

    return result;
  }

  public void Delete(ContentReference entity) {
    contentReferenceService.delete(entity);

    var result = contentReferenceService
        .findById(new ContentReferencePK(entity.getSource(), entity.getUid()))
        .orElse(null);
    if (result != null)
      throw new IllegalStateException("Entity must be deleted");
  }
}
