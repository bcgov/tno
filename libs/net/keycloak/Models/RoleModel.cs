using System;
using System.Collections.Generic;

namespace TNO.Keycloak.Models
{
    /// <summary>
    /// RoleModel class, provides a way to manage roles within keycloak.
    /// </summary>
    public class RoleModel
    {
        #region Properties
        /// <summary>
        /// get/set - The unique key that identifies this role.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// get/set - The unique name for this role.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// get/set - The description of the role.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// get/set - Whether this role belongs to a client.
        /// </summary>
        public bool ClientRole { get; set; }

        /// <summary>
        /// get/set - Whether this role is a composite role.
        /// </summary>
        public bool Composite { get; set; }

        /// <summary>
        /// get/set - The unique container Id for this role.
        /// </summary>
        public string? ContainerId { get; set; }

        /// <summary>
        /// get/set - A tree of composite roles.
        /// </summary>
        public RoleCompositeModel? Composites { get; set; }

        /// <summary>
        /// get/set - A dictionary of user attributes.
        /// </summary>
        public Dictionary<string, string[]>? Attributes { get; set; }
        #endregion
    }
}
