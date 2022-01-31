package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
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

  private final SessionFactory sessionFactory;
  private final IContentLinkRepository repository;

  /**
   * Creates a new instance of a ContentLinkService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The content link repository.
   */
  @Autowired
  public ContentLinkService(final SessionFactory sessionFactory, final IContentLinkRepository repository) {
    this.sessionFactory = sessionFactory;
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
   * Find all content link for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content link if it exists.
   */
  @Override
  public List<ContentLink> findById(int contentId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var results = session
          .createQuery("SELECT cl FROM ContentLink cl JOIN FETCH cl.link AS l WHERE cl.contentId=:id")
          .setParameter("id", contentId)
          .getResultList();
      return ListHelper.castList(ContentLink.class, results);
    } finally {
      ts.commit();
      session.close();
    }
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
   * Add a new content link to the data source.
   * 
   * @param entities An array of content link to add.
   * @return A new instance of the content link that was added.
   */
  @Override
  public Iterable<ContentLink> add(Iterable<ContentLink> entities) {
    var result = repository.saveAll(
        StreamSupport.stream(entities.spliterator(), false).map((entity) -> PrincipalHelper.addAudit(entity)).toList());
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
   * Update the specified content link in the data source.
   * 
   * @param entities An array of content link to update.
   * @return A new instance of the content link that was updated.
   */
  @Override
  public Iterable<ContentLink> update(Iterable<ContentLink> entities) {
    var result = repository.saveAll(StreamSupport.stream(entities.spliterator(), false)
        .map((entity) -> PrincipalHelper.updateAudit(entity)).toList());
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

  /**
   * Delete the specified content link from the data source.
   * 
   * @param entities An array of content link to delete.
   */
  @Override
  public void delete(Iterable<ContentLink> entities) {
    repository.deleteAll(entities);
  }

}
