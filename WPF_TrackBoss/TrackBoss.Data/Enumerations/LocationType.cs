using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2021 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible types of locations. This is a helper enumeration
    /// that enables quick access to a location's type.
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// Indicates location's type is unknown.
        /// </summary>
        [Description("Unknown Type")]
        Unknown = -1,

        /// <summary>
        /// Indicates location is a track.
        /// </summary>
        [Description("Track")]
        Track,

        /// <summary>
        /// Indicates location is a spur.
        /// </summary>
        [Description("Spur")]
        Spur,

        /// <summary>
        /// Indicates location is a city.
        /// </summary>
        [Description("City")]
        City,

        /// <summary>
        /// Indicates location is a yard.
        /// </summary>
        [Description("Yard")]
        Yard,

        /// <summary>
        /// Indicates location is an industry.
        /// </summary>
        [Description("Industry")]
        Industry,

        /// <summary>
        /// Indicates general property of location.
        /// </summary>
        [Description("General")]
        General,

        /// <summary>
        /// Indicates PrintScheme property of location.
        /// </summary>
        [Description("PrintScheme")]
        PrintScheme,
    }
}
