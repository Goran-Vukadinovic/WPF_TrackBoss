using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible blocking (as in assembling) actions which can be 
    /// executed at connection points for services.
    /// </summary>
    public enum BlockingConnectionPointAction
    {
        /// <summary>
        /// Indicates neither power nor caboose should be changed at connection
        /// points.
        /// </summary>
        [Description("Power and Caboose Not Changed")]
        PowerAndCabooseNotChanged,

        /// <summary>
        /// Indicates new power should be assigned at connection points.
        /// </summary>
        [Description("Assign New Power")]
        AssignNewPower,

        /// <summary>
        /// Indicates a new caboose should be assigned at connection points.
        /// </summary>
        [Description("Assign New Caboose")]
        AssignNewCaboose,

        /// <summary>
        /// Indicates new caboose and power should be assigned at connection
        /// points.
        /// </summary>
        [Description("Assign New Power and Caboose")]
        AssignNewPowerAndCaboose,
    }
}
