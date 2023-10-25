using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible point restrictions for works.
    /// </summary>
    public enum WorkPointRestriction
    {
        /// <summary>
        /// Indicates there is no restriction.
        /// </summary>
        [Description("None")]
        None,

        /// <summary>
        /// Indicates restriction is for trailing.
        /// </summary>
        [Description("Trailing")]
        Trailing,

        /// <summary>
        /// Indicates restriction is for facing.
        /// </summary>
        [Description("Facing")]
        Facing,
    }
}
