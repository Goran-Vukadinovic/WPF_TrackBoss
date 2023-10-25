using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TrackBoss.Model.Shared;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Windsor;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel
{
    /// <summary>
    /// Defines all properties and behaviors for a ViewModel which acts as a
    /// basic editor - i.e. supports new, save, delete, cancel, and close
    /// functionality.
    /// </summary>
    public abstract class EditorViewModel : ViewModelBase, IDisposable
    {
        #region Fields

        private readonly RelayCommand newCommandValue;

        private readonly RelayCommand saveCommandValue;

        private readonly RelayCommand deleteCommandValue;

        private readonly RelayCommand cancelCommandValue;

        private readonly AsyncCommand closeCommandValue;

        private StatusModel statusModel;

        private bool disposed;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        protected EditorViewModel()
        {
            // Initialize fields.
            this.statusModel = CastleWindsor.Default.Resolve<StatusModel>();

            // Hook-up commands.
            this.newCommandValue = new RelayCommand(this.NewCommandExecute, this.NewCommandCanExecute);
            this.saveCommandValue = new RelayCommand(this.SaveCommandExecute, this.SaveCommandCanExecute);
            this.deleteCommandValue = new RelayCommand(this.DeleteCommandExecute, this.DeleteCommandCanExecute);
            this.cancelCommandValue = new RelayCommand(this.CancelCommandExecute, this.CancelCommandCanExecute);
            this.closeCommandValue = new AsyncCommand(this.CloseCommandExecute, this.CloseCommandCanExecute);

            // Attach event handlers.
            this.statusModel.IsBusyChanged += this.StatusModel_IsBusyChanged;
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
                    this.statusModel.IsBusyChanged -= this.StatusModel_IsBusyChanged;
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

        #region Abstract Methods

        /// <summary>
        /// Helper function which should contain all code that is executed when
        /// the NewCommand.Execute() method is called.
        /// </summary>
        /// <param name="obj">The object supplied when the command was 
        /// called.</param>
        protected abstract void newCommandExecuteHelper(object obj);

        /// <summary>
        /// Helper function which should contain all code that is executed when
        /// the SaveCommand.Execute() method is called.
        /// </summary>
        /// <param name="obj">The object supplied when the command was 
        /// called.</param>
        protected abstract void saveCommandExecuteHelper(object obj);

        /// <summary>
        /// Helper function which should contain all code that is executed when
        /// the DeleteCommand.Execute() method is called.
        /// </summary>
        /// <param name="obj">The object supplied when the command was 
        /// called.</param>
        protected abstract void deleteCommandExecuteHelper(object obj);

        /// <summary>
        /// Helper function which should contain all code that is executed when
        /// the CancelCommand.Execute() method is called.
        /// </summary>
        /// <param name="obj">The object supplied when the command was 
        /// called.</param>
        protected abstract void cancelCommandExecuteHelper(object obj);

        /// <summary>
        /// Helper function which should contain all code that is executed when
        /// the CloseCommand.Execute() method is called.
        /// </summary>
        protected abstract Task closeCommandExecuteHelper();

        #endregion

        #region Private Methods

        /// <summary>
        /// Invalidates all commands on this object.
        /// </summary>
        protected virtual void invalidateAllCommands()
        {
            // Invalidate commands.
            this.newCommandValue.InvalidateCanExecuteChanged();
            this.saveCommandValue.InvalidateCanExecuteChanged();
            this.deleteCommandValue.InvalidateCanExecuteChanged();
            this.cancelCommandValue.InvalidateCanExecuteChanged();
            this.closeCommandValue.RaiseCanExecuteChanged();
        }

        #endregion

        #region Command Handlers

        protected virtual bool CloseCommandCanExecute()
        {
            // If application is busy, disallow. This is the default behavior.
            return !this.statusModel.IsBusy;
        }

        private async Task CloseCommandExecute()
        {
            // Make sure this is allowed.
            // Because the AsyncCommand enforces the CanExecute check automatically,
            // it is not called here.

            // Call helper method.
            await this.closeCommandExecuteHelper();
        }

        protected virtual bool CancelCommandCanExecute(object obj)
        {
            // If application is busy, disallow. This is the default behavior.
            return !this.statusModel.IsBusy;
        }

        private void CancelCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.CancelCommand.CanExecute(obj))
                return;

            // Call helper method.
            this.cancelCommandExecuteHelper(obj);
        }

        protected virtual bool DeleteCommandCanExecute(object obj)
        {
            // If application is busy, disallow. This is the default behavior.
            return !this.statusModel.IsBusy;
        }

        private void DeleteCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.DeleteCommand.CanExecute(obj))
                return;

            // Call helper method.
            this.deleteCommandExecuteHelper(obj);
        }

        protected virtual bool SaveCommandCanExecute(object obj)
        {
            // If application is busy, disallow. This is the default behavior.
            return !this.statusModel.IsBusy;
        }

        private void SaveCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.SaveCommand.CanExecute(obj))
                return;

            // Call helper method.
            this.saveCommandExecuteHelper(obj);
        }

        protected virtual bool NewCommandCanExecute(object obj)
        {
            // If application is busy, disallow. This is the default behavior.
            return !this.statusModel.IsBusy;
        }

        private void NewCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.NewCommand.CanExecute(obj))
                return;

            // Call helper method.
            this.newCommandExecuteHelper(obj);
        }

        #endregion

        #region Event Handlers

        private void StatusModel_IsBusyChanged(object sender, EventArgs e)
        {
            // Invalidate all commands.
            this.invalidateAllCommands();
        }

        #endregion

        #region Properties

        #region Commands

        public ICommand NewCommand
        {
            get { return this.newCommandValue; }
        }

        public ICommand SaveCommand
        {
            get { return this.saveCommandValue; }
        }

        public ICommand DeleteCommand
        {
            get { return this.deleteCommandValue; }
        }

        public ICommand CancelCommand
        {
            get { return this.cancelCommandValue; }
        }

        public IAsyncCommand CloseCommand
        {
            get { return this.closeCommandValue; }
        }

        #endregion

        #region Protected

        protected StatusModel StatusModel
        {
            get { return this.statusModel; }
        }

        #endregion

        #endregion
    }
}
