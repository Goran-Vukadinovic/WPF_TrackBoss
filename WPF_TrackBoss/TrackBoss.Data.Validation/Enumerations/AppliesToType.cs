using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.Enumerations
{
    /// <summary>
    /// Defines all possible types which can have data validation applied
    /// to them.
    /// </summary>
    public enum AppliesToType
    {
        [Description("Rolling Stock")]
        RollingStock,

        [Description("Car")]
        Car,

        [Description("Caboose")]
        Caboose,

        [Description("Locomotive")]
        Locomotive,

        [Description("City")]
        City,

        [Description("Yard")]
        Yard,
    }
}
