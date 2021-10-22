package ca.bc.gov.tno.dal.db.services;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.db.entities.DataSourceType;
import ca.bc.gov.tno.dal.db.repositories.IDataSourceTypeRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceTypeService;

@Service
public class DataSourceTypeService implements IDataSourceTypeService {

  @Autowired
  private IDataSourceTypeRepository repository;

  @Override
  public List<DataSourceType> findAll() {
    var dataSourceTypes = (List<DataSourceType>) repository.findAll();
    return dataSourceTypes;
  }

}
