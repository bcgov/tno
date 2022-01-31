package ca.bc.gov.tno.dal.db;

import java.util.HashMap;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;

@Component
public class DataSourceTest {
  IDataSourceService dataSourceService;

  @Autowired
  public DataSourceTest(final IDataSourceService dataSourceService) {
    this.dataSourceService = dataSourceService;
  }

  public void Run() {
    var result = Add();
    FindById(result);
    FindByCode(result);
    Delete(result);
  }

  public DataSource Add() {
    System.out.println("Fetching data sources");
    var sources = dataSourceService.findAll();
    sources.forEach(ds -> System.out.println(ds.getName() + " " + ds.getConnection()));

    var find = dataSourceService.findByCode("test");
    if (!find.isEmpty())
      throw new IllegalStateException();

    var add = new DataSource();
    add.setCode("ghi");
    add.setName("Global Hunger Index");
    add.setTopic("test");
    add.setConnection(new HashMap<String, Object>() {
      {
        put("url", "https://www.globalhungerindex.org/atom.xml");
      }
    });
    add.setMediaTypeId(1);
    add.setLicenseId(1);
    var result = dataSourceService.add(add);
    if (result.getId() == 0)
      throw new IllegalStateException();

    return result;
  }

  public Optional<DataSource> FindById(DataSource entity) {

    var result = dataSourceService.findById(entity.getId());
    if (result.isEmpty())
      throw new IllegalStateException();

    return result;
  }

  public Optional<DataSource> FindByCode(DataSource entity) {
    var result = dataSourceService.findByCode(entity.getCode());
    if (result.isEmpty())
      throw new IllegalStateException();

    return result;
  }

  public void Delete(DataSource entity) {
    dataSourceService.delete(entity);
  }
}
