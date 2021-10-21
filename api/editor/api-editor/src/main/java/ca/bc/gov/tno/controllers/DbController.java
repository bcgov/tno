package ca.bc.gov.tno.controllers;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.entities.User;

/**
 * Endpoints to communicate with Elasticsearch.
 */
@RestController
@RequestMapping("/db")
public class DbController {

	@Autowired
	private IUserService userService;

	@Autowired
	private IDataSourceService dataSourceService;

	/**
	 * Request a list of all users from the db.
	 *
	 * @return
	 */
	@GetMapping(path = "/users", produces = MediaType.APPLICATION_JSON_VALUE)
	public List<User> findUsers() {
		var users = userService.findAll();
		return users;
	}

	/**
	 * Request a list of all data sources from the db.
	 * 
	 * @return
	 */
	@GetMapping(path = "/data/sources", produces = MediaType.APPLICATION_JSON_VALUE)
	public List<DataSource> findDataSources() {
		var dataSources = dataSourceService.findAll();
		return dataSources;
	}

}
