package ca.bc.gov.tno.dal.db.repositories.interfaces;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.SourceMetric;

/**
 * IMetricRepository interface, provides a way to interact with the
 * SourceMetric repository.
 */
@Repository
public interface ISourceMetricRepository extends JpaRepository<SourceMetric, Integer> {
  /**
   * Find the source metric by it's name.
   * 
   * @param name The name to search for.
   * @return An Optional{SourceMetric} if found with the specified name.
   */
  Optional<SourceMetric> findByName(String name);
}