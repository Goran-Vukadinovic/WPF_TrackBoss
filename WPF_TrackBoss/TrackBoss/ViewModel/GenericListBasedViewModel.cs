using System;
using System.Collections.ObjectModel;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel
{
    public abstract class GenericListBasedViewModel<T> : EditorViewModel where T : class
    {
        #region Fields

        private ObservableCollection<T> viewModels;

        private T selectedViewModel;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        protected GenericListBasedViewModel()
        {
            // Initialize fields to their defaults.
            this.viewModels = new ObservableCollection<T>();
        }

        #endregion

        #region IDisposable Override

        protected override void Dispose(bool disposing)
        {
            // Perform base disposal.
            base.Dispose(disposing);

            // To insure that all code is executed below, base.disposed is
            // not checked.
            if (disposing)
            {
                // Unhook message handlers.

                // Dispose of ViewModels which need it.
                foreach (T viewModel in this.ViewModels)
                {
                    if (viewModel is IDisposable)
                        ((IDisposable)viewModel).Dispose();
                }
                this.viewModels.Clear();
            }
        }

        #endregion

        #region Private Methods

        protected virtual void selectionChangeHelper()
        {
            // Nothing to do here.
        }

        protected virtual void previewSelectionChangeHelper(T previousItem, T newItem)
        {
            // Nothing to do here.
        }

        #endregion

        #region Event Handlers

        #endregion

        #region Public Methods

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of child ViewModels.
        /// </summary>
        public ObservableCollection<T> ViewModels
        {
            get { return this.viewModels; }
        }

        /// <summary>
        /// Gets the currently selected ViewModel.
        /// </summary>
        public T SelectedViewModel
        {
            get { return this.selectedViewModel; }
            set
            {
                if (this.selectedViewModel != value)
                {
                    // Get current item.
                    T priorSelectedItem = this.selectedViewModel;

                    // Perform any associated preview work.
                    this.previewSelectionChangeHelper(priorSelectedItem, value);

                    // Set new selection.
                    this.selectedViewModel = value;

                    // Perform any associated work.
                    this.selectionChangeHelper();

                    // Notify.
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
