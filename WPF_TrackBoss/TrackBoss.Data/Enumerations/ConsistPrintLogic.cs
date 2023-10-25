using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Enumerations
{
    public enum ConsistPrintLogic
    {
        /// <summary>
        /// Do not use logic for this particular option.
        /// </summary>
        [Description("Do Not Use")]
        DoNotUse,

        /// <summary>
        /// Train list for yard will be created solely based on the "hard"
        /// build time.
        /// </summary>
        [Description("Use Build Time")]
        BuildTime,

        /// <summary>
        /// Use application logic to generate train list for yard.
        /// </summary>
        [Description("Use Application Logic")]
        AppLogic,

        /// <summary>
        /// Indicates user will need to "tell" TrackBoss to run the OP's 
        /// process.
        /// </summary>
        [Description("Manual")]
        Manual,
    }
}
