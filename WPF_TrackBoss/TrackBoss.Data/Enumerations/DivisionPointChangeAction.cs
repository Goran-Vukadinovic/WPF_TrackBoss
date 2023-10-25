using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all actions which can occur at a division point in a route.
    /// </summary>
    public enum DivisionPointChangeAction
    {
        ///// <summary>
        ///// Indicates no changes should be made at division points.
        ///// </summary>
        //[Description("None")]
        //None,

        /// <summary>
        /// Indicates power should be changed at division points.
        /// </summary>
        [Description("Change Power")]
        ChangePower,

        /// <summary>
        /// Indicates the caboose should be changed at division points.
        /// </summary>
        [Description("Change Caboose")]
        ChangeCaboose,

        /// <summary>
        /// Indicates the caboose and power should be changed at division 
        /// points.
        /// </summary>
        [Description("Change Power and Caboose")]
        ChangeCabooseAndPower,
    }
}
