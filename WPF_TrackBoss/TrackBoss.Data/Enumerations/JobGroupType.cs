using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all types of job groups.
    /// </summary>
    public enum JobGroupType
    {
        [Description("Extra")]
        Extra,

        [Description("Scheduled")]
        Scheduled,

        [Description("Switcher")]
        Switcher,

        [Description("User")]
        User,
    }
}
