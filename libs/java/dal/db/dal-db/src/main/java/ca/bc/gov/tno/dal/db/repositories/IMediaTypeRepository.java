package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.MediaType;

/**
 * IMediaTypeRepository interface, provides a way to interact with the
 * MediaType repository.
 */
@Repository
public interface IMediaTypeRepository extends CrudRepository<MediaType, Integer> {

}