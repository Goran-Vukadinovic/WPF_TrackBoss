using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Model.Enumerations
{
    /// <summary>
    /// Defines all designers present in the application.
    /// </summary>
    public enum Designer
    {
        [Description("Cars Designer")]
        Cars,

        [Description("Cabooses Designer")]
        Cabeese,

        [Description("Locomotives Designer")]
        Locomotives,

        [Description("Cities Designer")]
        Cities,

        [Description("Yards Designer")]
        Yards,

        [Description("Services Designer")]
        Services,

        [Description("Crew Designer")]
        Crew,

        [Description("Wheel Report Designer")]
        WheelReport,

        [Description("Operations Designer")]
        Operations,
    }
}
