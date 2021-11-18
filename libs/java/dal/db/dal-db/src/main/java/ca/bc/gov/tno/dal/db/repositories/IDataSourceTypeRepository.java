package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.DataSourceType;

/**
 * IDataSourceTypeRepository interface, provides a way to interact with the
 * DataSourceType repository.
 */
@Repository
public interface IDataSourceTypeRepository extends CrudRepository<DataSourceType, Integer> {

}