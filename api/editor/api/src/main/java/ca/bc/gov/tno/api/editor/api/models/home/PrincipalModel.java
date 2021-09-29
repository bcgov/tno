package ca.bc.gov.tno.api.editor.api.models.home;

import java.io.Serializable;
import java.security.Principal;
import java.util.Collection;

import org.keycloak.adapters.RefreshableKeycloakSecurityContext;
import org.keycloak.adapters.springsecurity.account.SimpleKeycloakAccount;
import org.keycloak.representations.AccessToken;
import org.springframework.security.core.Authentication;

public class PrincipalModel implements Serializable {
  public String uid;
  public String username;
  public String email;
  public Collection<String> roles;

  public PrincipalModel() {
  }

  public PrincipalModel(Principal principal) {
    this.uid = principal.getName();
  }

  public PrincipalModel(Authentication authentication) {
    this.uid = authentication.getName();
    SimpleKeycloakAccount details = (SimpleKeycloakAccount) authentication.getDetails();
    this.roles = details.getRoles();

    RefreshableKeycloakSecurityContext context = details.getKeycloakSecurityContext();
    AccessToken token = context.getToken();
    this.username = token.getPreferredUsername();
    this.email = token.getEmail();
  }
}
