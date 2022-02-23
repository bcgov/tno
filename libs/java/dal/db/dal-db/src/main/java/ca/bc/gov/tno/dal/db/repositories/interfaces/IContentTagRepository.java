package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentTag;
import ca.bc.gov.tno.dal.db.entities.ContentTagPK;

/**
 * IContentTagRepository interface, provides a way to interact with the
 * ContentTag repository.
 */
@Repository
public interface IContentTagRepository extends JpaRepository<ContentTag, ContentTagPK> {

}