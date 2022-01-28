package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentCategoryPK;

/**
 * IContentCategoryRepository interface, provides a way to interact with the
 * ContentCategory repository.
 */
@Repository
public interface IContentCategoryRepository extends CrudRepository<ContentCategory, ContentCategoryPK> {

}