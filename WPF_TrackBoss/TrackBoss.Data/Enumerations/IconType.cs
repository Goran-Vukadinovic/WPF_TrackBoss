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
    public enum IconType
    {
        [Description("Unknown Type")]
        Unknown = -1,

        [Description("Track")]
        Track,

        [Description("Spur")]
        Spur,

        [Description("City")]
        City,

        [Description("Yard")]
        Yard,

        [Description("Industry")]
        Industry,

        [Description("General")]
        General,

        [Description("PrintScheme")]
        PrintScheme,

        [Description("MultiSwitcher")]
        MultiSwitcher,
    }
}
