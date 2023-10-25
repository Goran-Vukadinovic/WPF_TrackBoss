using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    /// <summary>
    /// Defines all yard requests that can be used for extra board
    /// logic.
    /// </summary>
    public enum YardRequestOption
    {
        /// <summary>
        /// Indicates service can only be run with yard request.
        /// </summary>
        [Description("Run Only with Yard Request")]
        RunOnlyWithYardRequest,

        /// <summary>
        /// Indicates yard can generate train in queue.
        /// </summary>
        [Description("Yard Can Generate Train in Queue")]
        YardCanGenerateTrainInQueue,
    }
}
