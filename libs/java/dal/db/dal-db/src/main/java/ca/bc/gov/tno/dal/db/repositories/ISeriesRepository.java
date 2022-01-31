package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Series;

/**
 * ISeriesRepository interface, provides a way to interact with the
 * Series repository.
 */
@Repository
public interface ISeriesRepository extends CrudRepository<Series, Integer> {

}