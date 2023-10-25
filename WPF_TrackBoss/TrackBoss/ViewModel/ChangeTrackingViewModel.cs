using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TrackBoss.Shared.Events;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel
{
    /// <summary>
    /// Defines properties and behaviors for a ViewModel which tracks
    /// changes to its properties.
    /// </summary>
    public class ChangeTrackingViewModel : ViewModelBase
    {
        #region Fields

        private bool hasChanges;

        private bool changeTrackingStarted;

        private DateTime lastChanged;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use. Additionally, this calls the constructor for 
        /// ViewModelBase.
        /// </summary>
        public ChangeTrackingViewModel() : base()
        {
            // Initialize member fields.
            this.LastChanged = DateTime.Now;
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised whenever the application's Ops mode has changed.
        /// </summary>
        public event EventHandler<EventArgs<string>> ChangesMade;

        #endregion

        #region Private Event Dispatchers

        protected void OnChangesMade(string propertyName)
        {
            // Invoke event.
            EventArgs<string> e = new EventArgs<string>(propertyName);
            this.ChangesMade?.Invoke(this, e);
        }

        #endregion

        #region Override Methods
        public void NotifyPropertyChanged_OnlyControlState([CallerMemberName] string propertyName = "")
        {
            // If change tracking hasn't been started, exit.
            if (!this.changeTrackingStarted)
                return;

            // Perform base processing.
            base.NotifyPropertyChanged(propertyName);
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // If change tracking hasn't been started, exit.
            if (!this.changeTrackingStarted)
                return;

            // Perform base processing.
            base.NotifyPropertyChanged(propertyName);

            // Mark as having changes. Note: this will not cause a stack
            // overflow exception due to the notify property pattern.
            if (!propertyName.Equals("TaskStatus"))
            {
                this.HasChanges = true;

                // Raise event. NOTE: as originally written, this first checked to see if
                // the HasChanges property itself was being changed. Although understandable,
                // the use case for the OnHasChangesChanged event is predicated on any
                // property causing this event to fire, including the HasChanges property.
                // Top-level ViewModels use this event to track changes on child ViewModels.
                Debug.WriteLine(string.Format("{0}: {1}", this.GetType(), propertyName));
                this.OnChangesMade(propertyName);

                // Set last changed date/time. Calling the LastChanged property here
                // was a mistake.
                this.lastChanged = DateTime.Now;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begins change tracking. Note: by default, this method MUST
        /// BE called in order to start tracking changes.
        /// </summary>
        public void StartTrackingChanges()
        {
            this.changeTrackingStarted = true;
        }

        /// <summary>
        /// Stops change tracking and resets the HasChanges
        /// property to false.
        /// </summary>
        public void ResetChangeTracking()
        {
            this.changeTrackingStarted = false;
            this.hasChanges = false;
        }

        #endregion

        #region Properties

        public DateTime LastChanged
        {
            get { return this.lastChanged; }
            set
            {
                if (this.lastChanged != value)
                {
                    this.lastChanged = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets/sets whether or not this ViewModel has changes.
        /// </summary>
        public bool HasChanges
        {
            get { return this.hasChanges; }
            set
            {
                if(this.hasChanges != value)
                {
                    this.hasChanges = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
