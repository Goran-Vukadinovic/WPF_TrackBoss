using System;
using System.Windows.Input;
using TrackBoss.Mvvm.Shared.Commands;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2021 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Dialogs
{
    public class HasValidationErrorsDialogViewModel : ViewModelBase
    {
        #region Fields

        private readonly RelayCommand okCommandValue;

        private readonly RelayCommand cancelCommandValue;

        private string titleValue;

        private string messageValue;

        private bool showDialogOnValidationErrorsValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        private HasValidationErrorsDialogViewModel()
        {
            // Initialize member fields.
            this.okCommandValue = new RelayCommand(this.OKCommandExecute);
            this.cancelCommandValue = new RelayCommand(this.CancelCommandExecute);
        }

        /// <summary>
        /// Overload constructor. Initializes this object using the specified
        /// information.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="message">The extended information message to be displayed.</param>
        public HasValidationErrorsDialogViewModel(string title, string message) : this()
        {
            // Validate parameters.
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            // Assign fields.
            this.titleValue = title;
            this.messageValue = message;
            this.showDialogOnValidationErrorsValue = true;
        }

        #endregion

        #region Command Handlers

        private void CancelCommandExecute(object obj)
        {
            // Do Cancel processing.
            this.Cancelled = true;
        }

        private void OKCommandExecute(object obj)
        {
            // Do OK processing.
            this.Cancelled = false;
        }

        #endregion

        #region Properties

        public ICommand OKCommand
        {
            get => this.okCommandValue;
        }

        public ICommand CancelCommand
        {
            get => this.cancelCommandValue;
        }

        /// <summary>
        /// Gets/sets whether the don't show this again checkbox is
        /// checked or not.
        /// </summary>
        public bool Cancelled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/sets the title for this dialog.
        /// </summary>
        public string Title 
        { 
            get => this.titleValue;
            set
            {
                if(this.titleValue != value)
                {
                    this.titleValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets/sets the secondary validation message that is displayed
        /// below the "warning" line.
        /// </summary>
        public string Message 
        {
            get => this.messageValue;
            set
            {
                if(this.messageValue != value)
                {
                    this.messageValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets/sets whether the don't show this again checkbox is
        /// checked or not.
        /// </summary>
        public bool ShowDialogOnValidationErrors 
        {
            get => this.showDialogOnValidationErrorsValue;
            set
            {
                if(this.showDialogOnValidationErrorsValue != value)
                {
                    this.showDialogOnValidationErrorsValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion

    }
}
