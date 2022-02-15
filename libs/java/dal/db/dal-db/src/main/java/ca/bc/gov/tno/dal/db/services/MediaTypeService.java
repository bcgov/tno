package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.MediaType;
import ca.bc.gov.tno.dal.db.repositories.IMediaTypeRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IMediaTypeService;

/**
 * MediaTypeService class, provides a concrete way to interact with media
 * types in the database.
 */
@Service
public class MediaTypeService implements IMediaTypeService {
  private final SessionFactory sessionFactory;
  private final IMediaTypeRepository repository;

  /**
   * Creates a new instance of a MediaTypeService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The media type repository.
   */
  @Autowired
  public MediaTypeService(final SessionFactory sessionFactory, final IMediaTypeRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of media type.
   */
  @Override
  public List<MediaType> findAll() {
    var MediaTypes = (List<MediaType>) repository.findAll();
    return MediaTypes;
  }

  /**
   * Find the media type for the specified primary key.
   * 
   * @param key The primary key.
   * @return An instance of the media type if it exists.
   */
  @Override
  public Optional<MediaType> findById(int key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Find the media type for the specified primary key.
   * 
   * @param name The name of the media type.
   * @return An instance of the media type if it exists.
   */
  @Override
  public Optional<MediaType> findByName(String name) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var sql = """
          FROM MediaType mt
          WHERE mt.name = :name
          """;
      var query = session.createQuery(sql).setParameter("name", name).setMaxResults(1);
      return ListHelper.castList(MediaType.class, query.getResultList()).stream().findFirst();
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new media type to the data source.
   * 
   * @param entity The media type to add.
   * @return An instance of the media type that was added.
   */
  @Override
  public MediaType add(MediaType entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified media type in the data source.
   * 
   * @param entity The media type to update.
   * @return An instance of the media type that was updated.
   */
  @Override
  public MediaType update(MediaType entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified media type from the data source.
   * 
   * @param entity The media type to delete.
   */
  @Override
  public void delete(MediaType entity) {
    repository.delete(entity);
  }

}
