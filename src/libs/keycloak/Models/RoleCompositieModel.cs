using System;
using System.Collections.Generic;

namespace TNO.Keycloak.Models
{
    /// <summary>
    /// RoleCompositeModel class, provides a way to manage role composites within keycloak.
    /// </summary>
    public class RoleCompositeModel
    {
        #region Properties
        /// <summary>
        /// get/set - A dictionary of clients.
        /// </summary>
        public Dictionary<string, string[]>? Client { get; set; }

        /// <summary>
        /// get/set - An array of realms.
        /// </summary>
        public IEnumerable<string>? Realm { get; set; }
        #endregion
    }
}
