using System;
using System.Collections.Generic;

namespace TNO.Keycloak.Models
{
    /// <summary>
    /// GroupModel class, provides a way to manage groups within keycloak.
    /// </summary>
    public class GroupModel
    {
        #region Properties
        /// <summary>
        /// get/set - The unique key that identifies this group.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// get/set - The unique name for this group.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// get/set - The path to the group.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// get/set - A dictionary of client roles.
        /// </summary>
        public Dictionary<string, string[]>? ClientRoles { get; set; }

        /// <summary>
        /// get/set - An array of realm roles.
        /// </summary>
        public IEnumerable<string>? RealmRoles { get; set; }

        /// <summary>
        /// get/set - A dictionary of group attributes.
        /// </summary>
        public Dictionary<string, string[]>? Attributes { get; set; }

        /// <summary>
        /// get/set - An array of sub-groups.
        /// </summary>
        public GroupModel[]? SubGroups { get; set; }
        #endregion
    }
}
