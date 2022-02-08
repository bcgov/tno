package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
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

  private final SessionFactory sessionFactory;
  private final IContentToneRepository repository;

  /**
   * Creates a new instance of a ContentToneService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The content tone repository.
   */
  @Autowired
  public ContentToneService(final SessionFactory sessionFactory, final IContentToneRepository repository) {
    this.sessionFactory = sessionFactory;
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
   * Find all content tone for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content tone if it exists.
   */
  @Override
  public List<ContentTone> findById(int contentId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var results = session
          .createQuery("SELECT ct FROM ContentTone ct JOIN FETCH ct.tonePool AS tp WHERE ct.contentId=:id")
          .setParameter("id", contentId)
          .getResultList();
      return ListHelper.castList(ContentTone.class, results);
    } finally {
      ts.commit();
      session.close();
    }
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
   * Add a new content tone pool to the data source.
   * 
   * @param entities An array of content tone pool to add.
   * @return A new instance of the content tone pool that was added.
   */
  @Override
  public Iterable<ContentTone> add(Iterable<ContentTone> entities) {
    var result = repository.saveAll(
        StreamSupport.stream(entities.spliterator(), false).map((entity) -> PrincipalHelper.addAudit(entity)).toList());
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
   * Update the specified content tone pool in the data source.
   * 
   * @param entities An array of content tone pool to update.
   * @return A new instance of the content tone pool that was updated.
   */
  @Override
  public Iterable<ContentTone> update(Iterable<ContentTone> entities) {
    var result = repository.saveAll(StreamSupport.stream(entities.spliterator(), false)
        .map((entity) -> PrincipalHelper.updateAudit(entity)).toList());
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

  /**
   * Delete the specified content tone pool from the data source.
   * 
   * @param entities An array of content tone pool to delete.
   */
  @Override
  public void delete(Iterable<ContentTone> entities) {
    repository.deleteAll(entities);
  }

}
