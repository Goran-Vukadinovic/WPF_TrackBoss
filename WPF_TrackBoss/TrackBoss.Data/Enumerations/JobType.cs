using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   David Colvin and Chad R. Hearn 
 * Legal:      ©2013-2019 all rights reserved
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible job types.
    /// </summary>
    public enum JobType
    {
        [Description("Read-Only")]
        ReadOnly,

        [Description("User")]
        User,

        [Description("Application")]
        Application,
    }
}
