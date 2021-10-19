package ca.bc.gov.tno.controllers;

import java.io.IOException;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.entities.User;
import ca.bc.gov.tno.dal.services.IUserService;

/**
 * Endpoints to communicate with Elasticsearch.
 */
@RestController
@RequestMapping("/db")
public class DbController {

	@Autowired
	private IUserService userService;

	/**
	 * Request the Elasticsearch index page.
	 *
	 * @return
	 * @throws IOException
	 */
	@GetMapping(path = "/users", produces = MediaType.APPLICATION_JSON_VALUE)
	public List<User> findUsers() {
		var users = userService.findAll();
		return users;
	}

}
