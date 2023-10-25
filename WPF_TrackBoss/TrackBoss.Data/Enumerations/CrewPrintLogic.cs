using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    public enum CrewPrintLogic
    {
        /// <summary>
        /// Indicates application logic should be used for printing.
        /// </summary>
        [Description("Use App Logic")]
        AppLogic,

        /// <summary>
        /// Indicates printing will be carried out manually.
        /// </summary>
        [Description("Manual")]
        Manual,
    }
}
