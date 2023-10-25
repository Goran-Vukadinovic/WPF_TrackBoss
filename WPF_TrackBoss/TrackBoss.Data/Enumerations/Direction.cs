using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible orientations.
    /// </summary>
    public enum Direction
    {
        [Description("No Restriction")]
        NoRestriction,

        [Description("North")]
        North,

        [Description("South")]
        South,

        [Description("West")]
        West,

        [Description("East")]
        East,
    }
}
