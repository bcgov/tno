package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentType;

/**
 * IContentTypeRepository interface, provides a way to interact with the
 * ContentType repository.
 */
@Repository
public interface IContentTypeRepository extends CrudRepository<ContentType, Integer> {

}