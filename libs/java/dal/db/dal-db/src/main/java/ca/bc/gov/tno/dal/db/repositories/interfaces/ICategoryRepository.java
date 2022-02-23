package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Category;

/**
 * ICategoryRepository interface, provides a way to interact with the
 * Category repository.
 */
@Repository
public interface ICategoryRepository extends JpaRepository<Category, Integer> {

}