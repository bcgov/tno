package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.DataSourceType;

public interface IDataSourceTypeService {
  List<DataSourceType> findAll();

  Optional<DataSourceType> findById(Integer key);

  DataSourceType add(DataSourceType entity);

  DataSourceType update(DataSourceType entity);

  void delete(DataSourceType entity);
}
