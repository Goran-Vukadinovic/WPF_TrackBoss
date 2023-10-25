using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.Enumerations
{
    /// <summary>
    /// Defines all possible formats for data to appear in which requires
    /// validation.
    /// </summary>
    public enum DataFormat
    {
        [Description("String")]
        String,

        [Description("Date/Time")]
        DateTime,

        [Description("Integer")]
        Integer,

        [Description("Numeric, Decimal")]
        Float,
    }
}
