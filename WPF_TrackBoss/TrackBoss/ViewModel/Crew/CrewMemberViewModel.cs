using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackBoss.Data;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Shared;

namespace TrackBoss.ViewModel.Crew
{
    public class CrewMemberViewModel : ChangeTrackingViewModel
    {
        #region Fields
        private Crewmember crewMember;
        private PhotoViewModel photoViewModelValue;

        private bool disposed;
        #endregion

        #region Constructor(s)

        private CrewMemberViewModel() : base()
        {
            // Load defaults.
        }

        public CrewMemberViewModel(Crewmember crewMember) : this()
        {
            // Validate parameter.
            if (crewMember == null)
                throw new ArgumentNullException(nameof(crewMember));


            // Assign member fields.
            this.crewMember = crewMember;

            if (this.crewMember.Photo != null)
                this.photoViewModelValue = new PhotoViewModel(this.crewMember.Photo);
            else
            {
                Photo photo = new Photo();
                this.crewMember.Photo = photo;
                this.photoViewModelValue = new PhotoViewModel(photo);
            }

            // Hook-up event handlers.
            this.photoViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;

            // Initialize properties to their defaults.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }
        #endregion

        #region Public Methods
        public Crewmember ToCrewMember()
        {
            return crewMember;
        }
        #endregion

        #region Private Methods
        private void updateDisplayText()
        {
            this.DisplayText = this.crewMember.ToString();
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
                    this.photoViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
                    this.photoViewModelValue.Dispose();
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
        public long ID
        {
            get { return this.crewMember.ID; }
        }

        public string FirstName
        {
            get { return this.crewMember.FirstName; }
            set
            {
                if (this.crewMember.FirstName != value)
                {
                    this.crewMember.FirstName = value;
                    this.NotifyPropertyChanged();

                    // Update display text.
                    this.updateDisplayText();
                }
            }
        }

        public string Middle
        {
            get { return this.crewMember.Middle; }
            set
            {
                if (this.crewMember.Middle != value)
                {
                    this.crewMember.Middle = value;
                    this.NotifyPropertyChanged();

                    // Update display text.
                    this.updateDisplayText();
                }
            }
        }
        

        public string LastName
        {
            get { return this.crewMember.LastName; }
            set
            {
                if (this.crewMember.LastName != value)
                {
                    this.crewMember.LastName = value;
                    this.NotifyPropertyChanged();

                    // Update display text.
                    this.updateDisplayText();
                }
            }
        }

        public string EmailAddress
        {
            get { return this.crewMember.EmailAddress; }
            set
            {
                if (this.crewMember.EmailAddress != value)
                {
                    this.crewMember.EmailAddress = value;
                    this.NotifyPropertyChanged();

                    // Update display text.
                    this.updateDisplayText();
                }
            }
        }

        public string PhoneNumber
        {
            get { return this.crewMember.PhoneNumber; }
            set
            {
                if (this.crewMember.PhoneNumber != value)
                {
                    this.crewMember.PhoneNumber = value;
                    this.NotifyPropertyChanged();

                    // Update display text.
                    this.updateDisplayText();
                }
            }
        }

        public long? KammFactorMax
        {
            get { return this.crewMember.KammFactorMax; }
            set
            {
                if (this.crewMember.KammFactorMax != value)
                {
                    this.crewMember.KammFactorMax = value;
                    this.NotifyPropertyChanged();

                    // Update display text.
                    this.updateDisplayText();
                }
            }
        }

        public long? METMax
        {
            get { return this.crewMember.METMax; }
            set
            {
                if (this.crewMember.METMax != value)
                {
                    this.crewMember.METMax = value;
                    this.NotifyPropertyChanged();

                    // Update display text.
                    this.updateDisplayText();
                }
            }
        }

        public PhotoViewModel Photo
        {
            get { return this.photoViewModelValue; }
        }

        #endregion
    }
}
