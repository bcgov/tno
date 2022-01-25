package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Content;

/**
 * IContentRepository interface, provides a way to interact with the Content
 * repository.
 */
@Repository
public interface IContentRepository extends CrudRepository<Content, Integer> {

}