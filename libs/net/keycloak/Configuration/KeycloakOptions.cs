using TNO.Core.Exceptions;
using TNO.Core.Http.Configuration;
using System;
using System.ComponentModel.DataAnnotations;

namespace TNO.Keycloak.Configuration
{
    /// <summary>
    /// KeycloakOptions class, provides a way to configure keycloak.
    /// </summary>
    public class KeycloakOptions : AuthClientOptions
    {
        #region Properties
        /// <summary>
        /// get/set - The keycloak 'realm'.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "Configuration 'Realm' is required.")]
        public string Realm { get; set; } = "";
        #endregion

        #region Methods
        /// <summary>
        /// Validates the configuration for keycloak.
        /// </summary>
        /// <exception type="ConfigurationException">If the configuration property is invalid.</exception>
        public override void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.Realm))
                throw new ConfigurationException("The configuration for Keycloak:Realm is invalid or missing.");

            base.Validate();
        }
        #endregion
    }
}
