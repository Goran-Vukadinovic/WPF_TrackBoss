using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Model.Enumerations
{
    public enum TimeFormat
    {
        [Description("12 Hour")]
        Standard,

        [Description("24 Hour")]
        Military,
    }
}
