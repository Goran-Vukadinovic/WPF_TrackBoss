using System.ComponentModel;

/**
* Programmer: Chad R. Hearn
* Entities:   TrackBoss, LLC
* Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
*/

namespace TrackBoss.Model.Enumerations
{
    /// <summary>
    /// Defines all possible reasons for showing a designer.
    /// </summary>
    public enum ShowDesignerReason
    {
        /// <summary>
        /// Indicates designer should simply be shown.
        /// </summary>
        [Description("Show")]
        Show,

        /// <summary>
        /// Indicates designer should be shown and an existing item selected.
        /// </summary>
        [Description("History Item")]
        LaunchHistoryItem,

        /// <summary>
        /// Indicates designer should be shown and a new item created.
        /// </summary>
        [Description("Create Item")]
        CreateNewItem,
    }
}
