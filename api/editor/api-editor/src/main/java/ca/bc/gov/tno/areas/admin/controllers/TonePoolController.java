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

import ca.bc.gov.tno.dal.db.services.interfaces.ITonePoolService;
import ca.bc.gov.tno.dal.db.entities.TonePool;

/**
 * Endpoints to communicate with the TNO DB tonePools.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminTonePoolController")
@RequestMapping("/admin/tone/pools")
public class TonePoolController {

  /**
   * DAL for tonePool.
   */
  @Autowired
  private ITonePoolService tonePoolService;

  /**
   * Request a list of all tonePools from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<TonePool> findAll() {
    var results = tonePoolService.findAll();
    return results;
  }

  /**
   * Request a list of all tonePools from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public TonePool findById(@PathVariable(required = true) Integer id) {
    var TonePool = tonePoolService.findById(id).orElse(null);
    return TonePool;
  }

  /**
   * Add a new tonePool to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = "/", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public TonePool add(@RequestBody TonePool model) {
    var tonePool = tonePoolService.add(model);
    return tonePool;
  }

  /**
   * Update the tonePool in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public TonePool update(@PathVariable Integer id, @RequestBody TonePool model) {
    var tonePool = tonePoolService.add(model);
    return tonePool;
  }

  /**
   * Delete the tonePool from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public TonePool delete(@PathVariable Integer id, @RequestBody TonePool model) {
    tonePoolService.delete(model);
    return model;
  }

}
