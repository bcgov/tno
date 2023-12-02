using System.Collections.Generic;

namespace TNO.Ches.Models
{
    /// <summary>
    /// IEmailMerge interface, provides a structure to manage generating multiple emails with a single template.
    /// </summary>
    public interface IEmailMerge: IEmailBase
    {
        /// <summary>
        /// get/set - An array of template variables.
        /// </summary>
        IEnumerable<IEmailContext> Contexts { get; set; }
    }
}
