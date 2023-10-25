/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all possible crew instruction set types.
    /// </summary>
    public enum InstructionSetType
    {
        /// <summary>
        /// Indicates instruction describes origin instructions.
        /// </summary>
        Origin,

        /// <summary>
        /// Indicates instruction set describes enroute specific
        /// instructions.
        /// </summary>
        Enroute,

        /// <summary>
        /// Indicates instruction set describes termination instructions.
        /// </summary>
        Termination,
    }
}
