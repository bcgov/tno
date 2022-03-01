package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Series;

/**
 * ISeriesRepository interface, provides a way to interact with the
 * Series repository.
 */
@Repository
public interface ISeriesRepository extends JpaRepository<Series, Integer> {

}