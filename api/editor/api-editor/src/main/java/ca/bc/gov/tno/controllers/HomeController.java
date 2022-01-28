package ca.bc.gov.tno.controllers;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import org.springframework.http.MediaType;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.models.home.PrincipalModel;

@RestController
public class HomeController {

  @GetMapping(path = { "", "/" })
  public String index() {
    return "TNO API";
  }

  @GetMapping("/test/{id}")
  public String test(@PathVariable String id) {
    return id;
  }

  @GetMapping(value = "/userinfo", produces = MediaType.APPLICATION_JSON_VALUE)
  public String userinfo(Authentication authentication) throws JsonProcessingException {
    ObjectMapper mapper = new ObjectMapper();
    PrincipalModel model = new PrincipalModel(authentication);
    return mapper.writeValueAsString(model);
  }

}
