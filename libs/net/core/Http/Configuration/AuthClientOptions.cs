using TNO.Core.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace TNO.Core.Http.Configuration
{
    /// <summary>
    /// OpenIdConnectOptions class, provides a way to configure Open ID Connect.
    /// </summary>
    public class AuthClientOptions
    {
        #region Properties
        /// <summary>
        /// get/set - The open id connect 'authority' URL.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "Configuration 'Authority' is required.")]
        public string Authority { get; set; } = "";

        /// <summary>
        /// get/set - The open id connect 'audience' or the client id that identifies the API.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "Configuration 'Audience' is required.")]
        public string? Audience { get; set; }

        /// <summary>
        /// get/set - The open id connect client 'secret'.
        /// </summary>
        /// <value></value>
        public string? Secret { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Validates the configuration for keycloak.
        /// </summary>
        /// <exception type="ConfigurationException">If a configuration property is invald.</exception>
        public virtual void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.Authority))
                throw new ConfigurationException("The configuration for OpenIdConnect:Authority is invalid or missing.");
            if (String.IsNullOrWhiteSpace(this.Audience))
                throw new ConfigurationException("The configuration for OpenIdConnect:Audience is invalid or missing.");
        }
        #endregion
    }
}
