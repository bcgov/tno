package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentCategoryPK;

/**
 * IContentCategoryRepository interface, provides a way to interact with the
 * ContentCategory repository.
 */
@Repository
public interface IContentCategoryRepository extends JpaRepository<ContentCategory, ContentCategoryPK> {

}