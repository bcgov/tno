package ca.bc.gov.tno.controllers;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;
import ca.bc.gov.tno.models.home.PrincipalModel;

/**
 * AuthController class, provides general endpoints for the api.
 */
@PreAuthorize("isAuthenticated()")
@RestController("AuthController")
@RequestMapping({ "/auth", "/api/auth" })
public class AuthController {

  final private IUserService userService;

  /**
   * Creates a new instance of a AuthController object, initializes with specified
   * parameters.
   *
   * @param userService The user DAL service.
   */
  @Autowired
  public AuthController(final IUserService userService) {
    this.userService = userService;
  }

  /**
   * Return user info for the current user.
   *
   * @param authentication
   * @return
   * @throws JsonProcessingException
   */
  @GetMapping(path = { "/userinfo", "/userinfo/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public String userinfo(Authentication authentication) throws JsonProcessingException {
    ObjectMapper mapper = new ObjectMapper();
    PrincipalModel model = new PrincipalModel(authentication);

    var result = userService.findByUsername(model.getUsername());
    if (result.isPresent()) {
      var user = result.get();
      model.setId(user.getId());
      model.setLastLoginOn(user.getLastLoginOn());
      model.setDisplayName(user.getDisplayName());
    }
    return mapper.writeValueAsString(model);
  }

}
