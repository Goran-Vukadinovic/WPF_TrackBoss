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
using TrackBoss.Model.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Cabeese;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Switchers;
using static Syncfusion.Windows.Controls.SfNavigator;

namespace TrackBoss.ViewModel.Cities
{
    public class IndustryViewModel : ChangeTrackingViewModel, IDisposable
    {
        public CitiesDesignerViewModel Designer { get; }
        private SwitcherViewModel switcherViewModelValue;
        private PhotoViewModel photoViewModelValue;

        private string displayText = "";
        private ObservableCollection<string> directions;
        private long directionRestrictionEnum;

        public Industry CurIndustry;

        private bool useSwitcher;


        #region Constructors
        public IndustryViewModel(CitiesDesignerViewModel designerViewModel, Industry industry)
        {
            CurIndustry = industry;
            Designer = designerViewModel;


            directions = new ObservableCollection<string>();
            directions.Add("No Restriction");
            directions.Add("North");
            directions.Add("South");
            directions.Add("West");
            directions.Add("East");
            
            DirectionRestrictionEnum = (int)industry.DirectionRestrictionEnum;

            DisplayText = industry.ToString();

            if(industry.Switcher != null)
            {
                this.switcherViewModelValue = new SwitcherViewModel(this.CurIndustry.Switcher);
                this.switcherViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
                this.useSwitcher = true;
            }

            this.ChangesMade += this.IndustryViewModel_ChangesMade;

            if (this.CurIndustry.Site.Location.Photo != null)
                this.photoViewModelValue = new PhotoViewModel(this.CurIndustry.Site.Location.Photo);
            else
            {
                Photo photo = new Photo();
                this.CurIndustry.Site.Location.Photo = photo;
                this.photoViewModelValue = new PhotoViewModel(photo);
            }

            // Hook-up event handlers.
            this.photoViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;

            // Begin tracking changes.
            this.StartTrackingChanges();
        }
        #endregion

        #region private
        private void IndustryViewModel_ChangesMade(object sender, EventArgs e)
        {
            // Validate parameters.
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            // Invalidate commands.
            Designer.invalidateAllCommands_Industry();
        }

        private void ChildViewModel_ChangesMade(object sender, EventArgs<string> e)
        {
            // Mark this ViewModel as having changes.
            this.HasChanges = true;

            // Raise the changes made event.
            this.OnChangesMade(e.Value);
        }

        public void Dispose()
        {
            this.ChangesMade -= this.IndustryViewModel_ChangesMade;
            this.photoViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
            this.photoViewModelValue.Dispose();
        }
        #endregion

        #region Properties
        public int DirectionRestrictionEnum
        {
            get 
            { 
                return (int)this.directionRestrictionEnum; 
            }
            set 
            { 
                this.directionRestrictionEnum = value;
                CurIndustry.DirectionRestrictionEnum = value;

                // Notify.
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> Directions
        {
            get { return this.directions; }
        }

        public string DisplayText
        {
            get { return CurIndustry.Site.Location.Name; }
            set
            {
                if (CurIndustry.Site.Location.Name != value)
                {
                    CurIndustry.Site.Location.Name = value;

                    // Notify.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool IsSwitcher
        {
            get { return useSwitcher; }
            set
            {
                this.useSwitcher = value;

                if (this.useSwitcher)
                {
                    this.CurIndustry.Switcher = Switcher.Create();

                    var enumerator = Designer.PrintersValue.GetEnumerator();
                    enumerator.MoveNext(); // sets it to the first element
                    var firstElement = enumerator.Current;

                    this.CurIndustry.Switcher.AliasPrinter.Printer = (firstElement as PrinterViewModel).ToPrinter();
                    SwitcherView = new SwitcherViewModel(this.CurIndustry.Switcher);

                    SwitcherView.ChangesMade += ChildViewModel_ChangesMade;
                    this.NotifyPropertyChanged("SwitcherView");
                }
                else
                    this.CurIndustry.Switcher = null;

                // Notify.
                this.NotifyPropertyChanged();
            }
        }

        public SwitcherViewModel SwitcherView
        {
            get { return this.switcherViewModelValue; }
            set { this.switcherViewModelValue = value; }
        }

        public PhotoViewModel Photo
        {
            get
            {
                return photoViewModelValue;
            }
            set
            {
                photoViewModelValue = value;
            }
        }

        public long PrintOrder
        {
            get
            {
                return (long)CurIndustry.PrintOrder;
            }
            set
            {
                CurIndustry.PrintOrder = value;
            }
        }
        #endregion
    }
}
