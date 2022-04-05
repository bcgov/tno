package ca.bc.gov.tno.controllers;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

/**
 * HomeController class, provides general endpoints for the api.
 */
@RestController("HomeController")
@RequestMapping({ "/", "/api" })
public class HomeController {

  /**
   * Basic endpoint to identify the api
   *
   * @return A string message
   */
  @GetMapping(path = { "", "/" })
  public String index() {
    return "TNO API";
  }

}
