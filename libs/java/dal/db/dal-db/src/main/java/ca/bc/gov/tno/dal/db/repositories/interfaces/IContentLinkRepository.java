package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentLink;
import ca.bc.gov.tno.dal.db.entities.ContentLinkPK;

/**
 * IContentLinkRepository interface, provides a way to interact with the
 * ContentLink repository.
 */
@Repository
public interface IContentLinkRepository extends JpaRepository<ContentLink, ContentLinkPK> {

}