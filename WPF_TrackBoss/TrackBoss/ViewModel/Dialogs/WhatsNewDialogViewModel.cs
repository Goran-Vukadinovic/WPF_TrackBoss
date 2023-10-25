using System.IO;
using TrackBoss.Configuration.Enumerations;
using TrackBoss.Configuration.IO;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2021 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Dialogs
{
    public class WhatsNewDialogViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        public WhatsNewDialogViewModel()
        {
            // Load contents of release notes.
            this.Content = File.ReadAllText(FileUtilities.GetFullpath(SpecialFileName.ReleaseNotes));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the content of the release notes.
        /// </summary>
        public string Content
        {
            get;
        }

        #endregion
    }
}
