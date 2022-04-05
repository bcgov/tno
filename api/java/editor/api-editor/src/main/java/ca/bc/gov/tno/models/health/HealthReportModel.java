package ca.bc.gov.tno.models.health;

import java.io.Serializable;
import java.security.Principal;
import java.util.Collection;

import org.keycloak.adapters.RefreshableKeycloakSecurityContext;
import org.keycloak.adapters.springsecurity.account.SimpleKeycloakAccount;
import org.keycloak.representations.AccessToken;
import org.springframework.security.core.Authentication;

public class HealthReportModel implements Serializable {
  public String status;

  public HealthReportModel() {
  }

  public HealthReportModel(String status) {
    this.status = status;
  }
}
