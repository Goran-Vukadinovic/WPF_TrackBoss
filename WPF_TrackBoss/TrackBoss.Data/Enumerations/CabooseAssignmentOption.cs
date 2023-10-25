using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all logic assignment options for cabeese.
    /// </summary>
    public enum CabooseAssignmentOption
    {
        /// <summary>
        /// Indicates the service is not assigned a caboose.
        /// </summary>
        [Description("No Caboose")]
        NoCaboose,

        /// <summary>
        /// Indicates the service is assigned an assigned crew caboose.
        /// </summary>
        [Description("Assigned Crew")]
        AssignedCrew,

        /// <summary>
        /// Indicates application uses selected options to assign a caboose
        /// to the service.
        /// </summary>
        [Description("Application Assigned")]
        ApplicationAssigned,

        /// <summary>
        /// Indicates ?
        /// </summary>
        [Description("ID Assigned")]
        IDAssigned,
    }
}
