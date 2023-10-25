using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Switchers;

namespace TrackBoss.ViewModel.Crew
{
    internal class CrewViewModel : ChangeTrackingViewModel, IDisposable
    {
        #region Fields
        private CrewMemberViewModel crewMemberViewModel;

        private bool disposed;

        #endregion

        #region Constructors
        private CrewViewModel()
        {
            // Prepare dictionaries.

            // Hook-up commands.
        }

        public CrewViewModel(Crewmember crew) : this()
        {
            // Validate parameter.
            if (crew == null)
                throw new ArgumentNullException(nameof(crew));

            // Validate components. All of these are REQUIRED for a city
            // object to be considered valid.
            if (crew.FirstName == null || crew.LastName == null)
                throw new InvalidOperationException("The object is invalid.");

            // Assign member fields.
            this.crewMemberViewModel = new CrewMemberViewModel(crew);

            // Initialize properties to their defaults.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        public CrewViewModel(CrewDesignerViewModel designerViewModel, Crewmember crew) : this(crew)
        {
            // Validate designer.
            if (designerViewModel == null)
                throw new ArgumentNullException(nameof(designerViewModel));

            // Assign member fields.
            this.Designer = designerViewModel;

            // Hook-up event handlers.
            this.crewMemberViewModel.ChangesMade += this.ChildViewModel_ChangesMade;
        }
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Unhook event handlers.
                    if (this.crewMemberViewModel != null)
                        this.crewMemberViewModel.ChangesMade -= this.ChildViewModel_ChangesMade;

                    // Dispose of child ViewModels which need disposing.
                    // Currently, none require this.                        
                }
                this.disposed = true;
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region public Methods
        public CrewMemberViewModel ToCrew()
        {
            return this.crewMemberViewModel;
        }
        #endregion

        #region Private Methods
        private void updateDisplayText()
        {
            this.DisplayText = this.crewMemberViewModel.DisplayText;
        }
        #endregion

        #region Public Method
        /// <summary>
        /// Returns the string representation of this object using the 
        /// specified format.
        /// </summary>
        /// <param name="format">The format string to use. The string 
        /// may contain 1 or more of the following specifiers::
        /// 
        ///     "F" - First Name
        ///     "L" - Last Name
        /// 
        /// </param>
        /// <returns>The string representation of this object.</returns>
        public string ToString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                return this.ToString();

            StringBuilder builder = new StringBuilder();
            foreach (char token in format)
            {
                switch (token)
                {
                    case 'F':
                        builder.Append(this.crewMemberViewModel.FirstName);
                        break;

                    case 'L':
                        builder.Append(this.crewMemberViewModel.LastName);
                        break;
                    default:
                        builder.Append(token);
                        break;
                }
            }

            // Return composed string.
            return builder.ToString();
        }
        #endregion

        #region Event Handlers
        private void ChildViewModel_ChangesMade(object sender, EventArgs<string> e)
        {
            // Mark this ViewModel as having changes.
            this.HasChanges = true;

            // Raise the changes made event.
            this.OnChangesMade(e.Value);
        }
        #endregion

        #region Properties
        public CrewDesignerViewModel Designer { get; }

        public CrewMemberViewModel CrewMemViewModel
        {
            get
            {
                return this.crewMemberViewModel;
            }
        }

        #endregion
    }
}
