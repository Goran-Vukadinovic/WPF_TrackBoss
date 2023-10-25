using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2013-2015 all rights reserved
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible car status dispositions used by TrackBoss.
    /// </summary>
    public enum CarStatusDisposition
    {
        [Description("Status Change at Industry")]
        StatusChangeAtIndustry,

        [Description("Status on Departure is Random")]
        StatusChangeIsRandom
    }
}
