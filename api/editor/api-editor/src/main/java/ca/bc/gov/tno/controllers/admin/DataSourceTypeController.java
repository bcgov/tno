package ca.bc.gov.tno.controllers.admin;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceTypeService;
import ca.bc.gov.tno.dal.db.entities.DataSourceType;

/**
 * Endpoints to communicate with the TNO DB data source types.
 */
@RolesAllowed("administrator")
@RestController
@RequestMapping("/admin/data/source/types")
public class DataSourceTypeController {

	@Autowired
	private IDataSourceTypeService dataSourceTypeService;

	/**
	 * Request a list of all data source types from the db.
	 *
	 * @return
	 */
	@GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
	public List<DataSourceType> findAll() {
		var dataSourceType = dataSourceTypeService.findAll();
		return dataSourceType;
	}

	/**
	 * Request a list of all data source types from the db.
	 *
	 * @param id The primary key.
	 * @return
	 */
	@GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
	public DataSourceType findById(@PathVariable(required = true) Integer id) {
		var dataSourceType = dataSourceTypeService.findById(id).orElse(null);
		return dataSourceType;
	}

	/**
	 * Add a new user to the db.
	 * 
	 * @param model
	 * @return
	 */
	@PostMapping(path = "/", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
	public DataSourceType add(@RequestBody DataSourceType model) {
		var dataSourceType = dataSourceTypeService.add(model);
		return dataSourceType;
	}

	/**
	 * Update the user in the db.
	 * 
	 * @param id    The primary key.
	 * @param model
	 * @return
	 */
	@PutMapping(path = "/{id}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
	public DataSourceType update(@PathVariable Integer id, @RequestBody DataSourceType model) {
		var dataSourceType = dataSourceTypeService.add(model);
		return dataSourceType;
	}

	/**
	 * Delete the user from the db.
	 * 
	 * @param id    The primary key.
	 * @param model
	 * @return
	 */
	@DeleteMapping(path = "/{id}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
	public DataSourceType delete(@PathVariable Integer id, @RequestBody DataSourceType model) {
		dataSourceTypeService.delete(model);
		return model;
	}

}
