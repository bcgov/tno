package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Category;

/**
 * ICategoryRepository interface, provides a way to interact with the
 * Category repository.
 */
@Repository
public interface ICategoryRepository extends CrudRepository<Category, Integer> {

}