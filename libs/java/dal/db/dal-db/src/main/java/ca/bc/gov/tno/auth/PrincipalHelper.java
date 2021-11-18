package ca.bc.gov.tno.auth;

import java.util.UUID;

import org.keycloak.adapters.springsecurity.account.SimpleKeycloakAccount;
import org.springframework.security.core.context.SecurityContextHolder;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * PrincipalHelper class, provides helper methods for a Principal.
 */
public class PrincipalHelper {

  /**
   * Extract the principal's name (uid).
   * 
   * @return The user's unique name identifier.
   */
  public static UUID getUserUid() {
    var authentication = SecurityContextHolder.getContext().getAuthentication();

    if (authentication != null) {
      var uid = authentication.getName();

      // TODO: Need to handle different types of user names.
      if (uid != null && uid.length() > 0)
        return UUID.fromString(uid);
    }

    return UUID.fromString("00000000-0000-0000-0000-000000000000");
  }

  /**
   * Extract the principal's preferred username.
   * 
   * @return Users preferred name.
   */
  public static String getUserName() {
    var authentication = SecurityContextHolder.getContext().getAuthentication();

    if (authentication != null) {
      SimpleKeycloakAccount details = (SimpleKeycloakAccount) authentication.getDetails();
      if (details != null) {
        var context = details.getKeycloakSecurityContext();
        var token = context.getToken();

        if (token != null) {
          var name = token.getPreferredUsername();

          if (name != null && name.length() > 0)
            return name;
        }
      }
    }

    return "";
  }

  /**
   * Update the created audit columns.
   * 
   * @param <T>    The typeof audit entity.
   * @param entity The entity to update the created columns.
   * @return The updated entity.
   */
  public static <T extends AuditColumns> T addAudit(T entity) {
    entity.setCreatedById(PrincipalHelper.getUserUid());
    entity.setCreatedBy(PrincipalHelper.getUserName());
    return updateAudit(entity);
  }

  /**
   * Update the updated audit columns.
   * 
   * @param <T>    The type of audit entity.
   * @param entity The entity to update the updated columns.
   * @return The updated entity.
   */
  public static <T extends AuditColumns> T updateAudit(T entity) {
    entity.setUpdatedById(PrincipalHelper.getUserUid());
    entity.setUpdatedBy(PrincipalHelper.getUserName());
    return entity;
  }
}
