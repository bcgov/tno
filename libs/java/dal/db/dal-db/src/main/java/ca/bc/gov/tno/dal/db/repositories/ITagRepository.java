package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Tag;

/**
 * ITagRepository interface, provides a way to interact with the
 * Tag repository.
 */
@Repository
public interface ITagRepository extends CrudRepository<Tag, String> {

}