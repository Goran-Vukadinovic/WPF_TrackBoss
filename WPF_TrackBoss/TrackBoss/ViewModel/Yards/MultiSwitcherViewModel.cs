using GalaSoft.MvvmLight;
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
using TrackBoss.ViewModel.Cabeese;
using TrackBoss.ViewModel.Cities;
using TrackBoss.ViewModel.Switchers;
using static Syncfusion.Windows.Controls.SfNavigator;

namespace TrackBoss.ViewModel.Yards
{
    public class MultiSwitcherViewModel : ChangeTrackingViewModel, IDisposable
    {
        #region Field
        private MultiSwitcher MultiSwitcher;
        public YardsDesignerViewModel Designer { get; }

        private SwitcherViewModel switcherViewModelValue;

        private bool disposed;
        #endregion

        #region Constructors
        public MultiSwitcherViewModel(YardsDesignerViewModel designerViewModel, MultiSwitcher multiSwitcher)
        {

            this.Designer = designerViewModel;
            this.MultiSwitcher = multiSwitcher;

            if (this.MultiSwitcher != null)
            {
                this.switcherViewModelValue = new SwitcherViewModel(this.MultiSwitcher.Switcher);
                this.switcherViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
                SwitcherView.ChangesMade += ChildViewModel_ChangesMade;
            }

            // Begin tracking changes.
            this.StartTrackingChanges();
        }
        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Unhook event handlers.
                    if (this.switcherViewModelValue != null)
                        this.switcherViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
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
        public SwitcherViewModel SwitcherView
        {
            get { return this.switcherViewModelValue; }
            set { this.switcherViewModelValue = value; }
        }

        public MultiSwitcherFunction? FunctionEnum
        {
            get
            {
                if (this.MultiSwitcher.FunctionEnum == null)
                    return null;
                return (MultiSwitcherFunction)this.MultiSwitcher.FunctionEnum;
            }
            set
            {
                this.MultiSwitcher.FunctionEnum = (long)value;
                this.NotifyPropertyChanged();
            }
        }

        public MultiSwitcherOption? OptionEnum
        {
            get
            {
                if (this.MultiSwitcher.OptionEnum == null)
                    return null;
                return (MultiSwitcherOption)this.MultiSwitcher.OptionEnum;
            }
            set
            {
                this.MultiSwitcher.OptionEnum = (long)value;
                this.NotifyPropertyChanged();
            }
        }
        #endregion
    }
}
