package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.DataSource;

public interface IDataSourceService {
  List<DataSource> findAll();

  Optional<DataSource> findById(Integer key);

  DataSource add(DataSource entity);

  DataSource update(DataSource entity);

  void delete(DataSource entity);
}
