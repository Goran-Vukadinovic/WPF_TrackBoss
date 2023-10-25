using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.Enumerations
{
    public enum ValidationSeverity
    {
        [Description("Warning")]
        Warning,

        [Description("Error")]
        Error,
    }
}
