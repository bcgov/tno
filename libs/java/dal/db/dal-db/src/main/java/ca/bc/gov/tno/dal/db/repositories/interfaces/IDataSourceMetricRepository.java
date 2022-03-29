package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.DataSourceMetric;
import ca.bc.gov.tno.dal.db.entities.DataSourceMetricPK;

/**
 * IDataSourceMetricRepository interface, provides a way to interact with the
 * DataSourceMetric repository.
 */
@Repository
public interface IDataSourceMetricRepository extends JpaRepository<DataSourceMetric, DataSourceMetricPK> {

}