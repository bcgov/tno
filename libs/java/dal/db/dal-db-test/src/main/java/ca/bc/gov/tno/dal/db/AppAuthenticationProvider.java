package ca.bc.gov.tno.dal.db;

import java.util.ArrayList;
import java.util.UUID;

import org.springframework.security.authentication.AuthenticationProvider;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.AuthenticationException;
import org.springframework.stereotype.Component;

@Component
public class AppAuthenticationProvider implements AuthenticationProvider {

  @Override
  public Authentication authenticate(Authentication authentication) throws AuthenticationException {
    String name = authentication.getName();
    String password = authentication.getCredentials().toString();

    if (name != null) {
      // use the credentials
      // and authenticate against the third-party system
      return new UsernamePasswordAuthenticationToken(UUID.fromString("00000000-0000-0000-0000-000000000000"), password,
          new ArrayList<>());
    } else {
      return null;
    }
  }

  @Override
  public boolean supports(Class<?> authentication) {
    return authentication.equals(UsernamePasswordAuthenticationToken.class);
  }

}
