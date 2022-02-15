package ca.bc.gov.tno.areas.admin.controllers;

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

import ca.bc.gov.tno.dal.db.services.interfaces.IClaimService;
import ca.bc.gov.tno.dal.db.entities.Claim;

/**
 * Endpoints to communicate with the TNO DB claims.
 */
@RolesAllowed("administrator")
@RestController("AdminClaimController")
@RequestMapping({ "/admin/claims", "/api/admin/claims" })
public class ClaimController {

  /**
   * DAL for claims.
   */
  private IClaimService claimService;

  /**
   * Creates a new instance of a ClaimController object.
   *
   * @param claimService
   */
  @Autowired
  public ClaimController(final IClaimService claimService) {
    this.claimService = claimService;
  }

  /**
   * Request a list of all claims from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<Claim> findAll() {
    var claims = claimService.findAll();
    return claims;
  }

  /**
   * Request a list of all claims from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Claim findById(@PathVariable(required = true) final Integer id) {
    var Claim = claimService.findById(id).orElse(null);
    return Claim;
  }

  /**
   * Add a new claim to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Claim add(@RequestBody final Claim model) {
    var claim = claimService.add(model);
    return claim;
  }

  /**
   * Update the claim in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Claim update(@PathVariable final Integer id, @RequestBody final Claim model) {
    var claim = claimService.update(model);
    return claim;
  }

  /**
   * Delete the claim from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Claim delete(@PathVariable final Integer id, @RequestBody final Claim model) {
    claimService.delete(model);
    return model;
  }

}
