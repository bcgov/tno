using System;
using System.Collections.Generic;
using System.Linq;

namespace TNO.Keycloak.Models
{
    /// <summary>
    /// UserModel class, provides a way to manage users within keycloak.
    /// </summary>
    public class UserModel
    {
        #region Properties
        /// <summary>
        /// get/set - The unique key that identifies this user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// get/set - The unique user name for this user.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// get/set - The user's given name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// get/set - The user's surname.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// get/set - The user's email.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// get/set - Whether the user's email has been verified.
        /// </summary>
        public bool? EmailVerified { get; set; }

        /// <summary>
        /// get/set - Whether the user is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// get/set - An array of realm roles the user belongs to.
        /// </summary>
        public string[]? RealmRoles { get; set; }

        /// <summary>
        /// get/set - An array of client roles the user belongs to.
        /// </summary>
        public string[]? ClientRoles { get; set; }

        /// <summary>
        /// get/set - An array of groups the user belongs to.
        /// </summary>
        public string[]? Groups { get; set; }

        /// <summary>
        /// get/set - A dictionary of user attributes.
        /// </summary>
        public Dictionary<string, string[]>? Attributes { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Get the display name from the attributes.
        /// </summary>
        /// <returns></returns>
        public string? GetDisplayName()
        {
            if (this.Attributes?.ContainsKey("displayName") == true)
                return this.Attributes?["displayName"]?.FirstOrDefault();
            else if (this.Attributes?.ContainsKey("display_name") == true)
                return this.Attributes?["display_name"]?.FirstOrDefault();
            else if (this.Attributes?.ContainsKey("name") == true)
                return this.Attributes?["name"]?.FirstOrDefault();

            return null;
        }

        /// <summary>
        /// Set the display name in the attributes.
        /// </summary>
        /// <param name="name"></param>
        public void SetDisplayName(string name)
        {
            if (this.Attributes?.ContainsKey("displayName") == true)
                this.Attributes["displayName"] = new[] { name };
            if (this.Attributes?.ContainsKey("display_name") == true)
                this.Attributes["display_name"] = new[] { name };
            if (this.Attributes?.ContainsKey("name") == true)
                this.Attributes["name"] = new[] { name };
        }
        #endregion
    }
}
