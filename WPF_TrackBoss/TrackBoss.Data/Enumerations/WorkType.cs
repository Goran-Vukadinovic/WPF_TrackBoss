using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible work types for a stop in a route.
    /// </summary>
    public enum WorkType
    {
        /// <summary>
        /// Indicates origin.
        /// </summary>
        [Description("Origin")]
        Origin,

        /// <summary>
        /// Indicates destination.
        /// </summary>
        [Description("Destination")]
        Destination,

        /// <summary>
        /// Indicates all work types are allowed.
        /// </summary>
        [Description("All")]
        All,

        /// <summary>
        /// Indicates only set-outs are allowed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SetOuts")]
        [Description("Set-outs")]
        SetOuts,

        /// <summary>
        /// Indicates only pick-ups are allowed.
        /// </summary>
        [Description("Pick-ups")]
        Pickups,
    }
}
