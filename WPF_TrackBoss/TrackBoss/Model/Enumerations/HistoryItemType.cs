using System.ComponentModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC
 * Legal:      ©2012-2019 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Model.Enumerations
{
    /// <summary>
    /// Defines the possible types for a history item within the application.
    /// </summary>
    public enum HistoryItemType
    {
        /// <summary>
        /// History item is a recent crew member.
        /// </summary>
        [Description("Recent Crew")]
        Crew,

        /// <summary>
        /// History item is a recent car audit.
        /// </summary>
        [Description("Recent Cars")]
        Car,

        /// <summary>
        /// History item is a recent caboose audit.
        /// </summary>
        [Description("Recent Cabooses")]
        Caboose,

        /// <summary>
        /// History item is a recent locomotive audit.
        /// </summary>
        [Description("Recent Locomotives")]
        Locomotive,

        /// <summary>
        /// History item is a recent city audit.
        /// </summary>
        [Description("Recent Cities")]
        City,

        /// <summary>
        /// History item is a recent industry audit.
        /// </summary>
        [Description("Recent Industries")]
        Industry,

        /// <summary>
        /// History item is a recent yard/station audit.
        /// </summary>
        [Description("Recent Yards")]
        Yard,

        /// <summary>
        /// History item is a recent spur audit.
        /// </summary>
        [Description("Recent Spurs")]
        Spur,

        /// <summary>
        /// History item is a recent movement.
        /// </summary>
        [Description("Recent Movements")]
        Movement,

        /// <summary>
        /// History item is a recent report.
        /// </summary>
        [Description("Recent Reports")]
        Report,

        /// <summary>
        /// History item is a recent service.
        /// </summary>
        [Description("Recent Services")]
        Service,

        /// <summary>
        /// History item is a recent configuration item.
        /// </summary>
        [Description("Recent Configuration Item")]
        Configuration,
    }
}
