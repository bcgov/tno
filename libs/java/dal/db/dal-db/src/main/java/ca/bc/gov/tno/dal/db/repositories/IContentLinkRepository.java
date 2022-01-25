package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentLink;
import ca.bc.gov.tno.dal.db.entities.ContentLinkPK;

/**
 * IContentLinkRepository interface, provides a way to interact with the
 * ContentLink repository.
 */
@Repository
public interface IContentLinkRepository extends CrudRepository<ContentLink, ContentLinkPK> {

}