using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TrackBoss.Data;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cars
{
    public class CarViewModel : ChangeTrackingViewModel, IDisposable
    {
        #region Fields

        private readonly RelayCommand updateCommodityStatusCommandValue;

        private CarsDesignerViewModel designerViewModel;

        private Car car;

        private RollingStockViewModel rollingStockViewModelValue;

        private CarTypeViewModel carTypeViewModelValue;

        private ScaleDataViewModel scaleDataViewModelValue;

        private LoadsEmptyViewModel loadsEmptyViewModelValue;

        private bool shouldDisposeCarType;

        private bool disposed;

        #endregion

        #region Constructor(s)

        private CarViewModel()
        {
            // Hook-up commands.
            this.updateCommodityStatusCommandValue = new RelayCommand(this.UpdateCommodityStatusCommandExecute, this.UpdateCommodityStatusCommandCanExecute);
        }

        public CarViewModel(Car car) : this()
        {
            // Validate parameter.
            if (car == null)
                throw new ArgumentNullException(nameof(car));

            // Validate components. All of these are REQUIRED for a car
            // object to be considered valid.
            if (car.RollingStock == null)
                throw new InvalidOperationException("The rolling stock object is invalid.");
            if (car.ScaleData == null)
                throw new InvalidOperationException("The scale data object is invalid.");
            if (car.LoadsEmpty == null)
                throw new InvalidOperationException("The loads/empty object is invalid.");

            // Assign member fields.
            this.car = car;
            this.rollingStockViewModelValue = new RollingStockViewModel(this.car.RollingStock);
            this.scaleDataViewModelValue = new ScaleDataViewModel(this.car.ScaleData);
            this.loadsEmptyViewModelValue = new LoadsEmptyViewModel(this.car.LoadsEmpty);
            if (this.car.CarType != null)
            {
                this.carTypeViewModelValue = new CarTypeViewModel(this.car.CarType);
                this.shouldDisposeCarType = true;
            }

            // Hook-up event handlers.
            this.rollingStockViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
            this.scaleDataViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
            this.loadsEmptyViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;

            // Initialize properties to their defaults.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        public CarViewModel(CarsDesignerViewModel designerViewModel, Car car) : this(car)
        {
            // Validate designer.
            if (designerViewModel == null)
                throw new ArgumentNullException(nameof(designerViewModel));

            // Assign member fields.
            this.designerViewModel = designerViewModel;
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
                    this.rollingStockViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
                    this.scaleDataViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
                    this.loadsEmptyViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;

                    // Dispose of child ViewModels that need disposing.
                    this.rollingStockViewModelValue.Dispose();
                    if(this.shouldDisposeCarType)
                        this.carTypeViewModelValue.Dispose();
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

        #region Private Methods

        private void updateDisplayText()
        {
            this.DisplayText = this.car.ToString();
        }

        #endregion

        #region Command Handlers
        
        private bool UpdateCommodityStatusCommandCanExecute(object obj)
        {
            // Validate and determine if command is allowed.
            CommodityViewModel commodity = obj as CommodityViewModel;
            return (commodity != null);
        }

        private void UpdateCommodityStatusCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.UpdateCommodityStatusCommand.CanExecute(obj))
                return;

            // Update status of commodity.
            CommodityViewModel commodityToUpdate = obj as CommodityViewModel;
            CarAssignedCommodity assignedCommodity = this.car.CarAssignedCommodities.FirstOrDefault(x => x.Commodity.ID == commodityToUpdate.ID);
            if(commodityToUpdate.IsSelected && assignedCommodity == null)
            {
                CarAssignedCommodity newAssignedCommodity = new
                    CarAssignedCommodity
                    {
                        Car = this.car,
                        Commodity = commodityToUpdate.ToCommodity(),
                    };
                this.car.CarAssignedCommodities.Add(newAssignedCommodity);
            }
            else if(!commodityToUpdate.IsSelected && assignedCommodity != null)
                this.car.CarAssignedCommodities.Remove(assignedCommodity);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }

        #endregion

        #region Event Handlers

        private void ChildViewModel_ChangesMade(object sender, EventArgs<string> e)
        {
            // Mark this object as having changes.
            this.HasChanges = true;

            // If the number changes, update the display text.
            bool changesMadeRaised = false;
            if (sender is RollingStockViewModel)
            {
                if (e.Value == nameof(RollingStockViewModel.Number) || e.Value == nameof(RollingStockViewModel.Road))
                {
                    this.updateDisplayText();
                    changesMadeRaised = true;
                }
            }

            // If the changes made event hasn't been raised, raise it.
            if (!changesMadeRaised)
                this.OnChangesMade(e.Value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the car data object encapsulated by this ViewModel.
        /// </summary>
        /// <returns>The encapsulated car data object.</returns>
        public Car ToCar()
        {
            return this.car;
        }

        /// <summary>
        /// Returns a list of integers representing the ID's of the commodities
        /// that are assigned to this car.
        /// </summary>
        /// <returns>The list of ID's of assigned commodities.</returns>
        public int[] GetAssignedCommodities()
        {
            return this.car.CarAssignedCommodities.Select(x => (int)x.Commodity.ID).ToArray();
        }

        /// <summary>
        /// Returns the string representation of this object using the 
        /// specified format.
        /// </summary>
        /// <param name="format">The format string to use. The string 
        /// may contain 1 or more of the following specifiers::
        /// 
        ///     "N" - Number
        ///     "C" - Color
        ///     "L" - Length
        ///     "T" - Car Type
        ///     "R" - Road
        ///     "O" - Owner
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
                    case 'N':
                        builder.Append(this.RollingStock.Number);
                        break;

                    case 'C':
                        builder.Append(this.RollingStock.Color);
                        break;

                    case 'L':
                        builder.Append(this.RollingStock.ScaleLength.HasValue ? this.RollingStock.ScaleLength.Value.ToString() : null);
                        break;

                    case 'T':
                        builder.Append(this.CarType != null ? this.CarType.DisplayText : null);
                        break;

                    case 'R':
                        builder.Append(this.RollingStock.Road != null ? this.RollingStock.Road.DisplayText : null);
                        break;

                    case 'O':
                        builder.Append(this.RollingStock.Owner != null ? this.RollingStock.Owner.DisplayText : null);
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

        #region Properties

        public ICommand UpdateCommodityStatusCommand
        {
            get { return this.updateCommodityStatusCommandValue; }
        }

        public CarsDesignerViewModel Designer
        {
            get { return this.designerViewModel; }
        }

        public long ID
        {
            get { return this.car.ID; }
        }

        public string UniqueId
        {
            get { return this.car.UniqueId; }
        }

        #region Child ViewModels

        public RollingStockViewModel RollingStock
        {
            get { return this.rollingStockViewModelValue; }
        }

        public CarTypeViewModel CarType
        {
            get { return this.carTypeViewModelValue; }
            set
            {
                // Assignment to null SHOULD NOT be allowed. This was
                // causing all sorts of UI problems, and logically this
                // should not be allowed. This goes for other entities
                // like Road, Owner, etc.
                if (value == null)
                    return;

                if(this.carTypeViewModelValue != value)
                {
                    // Dispose of car type if it belongs to this object.
                    if (this.shouldDisposeCarType)
                    {
                        this.carTypeViewModelValue.Dispose();
                        this.shouldDisposeCarType = false;
                    }

                    // Set new type.
                    this.carTypeViewModelValue = value;

                    // Assign new type.
                    this.car.CarType = this.carTypeViewModelValue.ToCarType();


                    if(Designer.IsNotMultiSelection == false)
                    {
                        foreach (CarViewModel item in Designer.selectedViewModels)
                        {
                            // Set new type.
                            item.carTypeViewModelValue = value;

                            // Assign new type.
                            item.car.CarType = this.carTypeViewModelValue.ToCarType();
                            item.NotifyPropertyChanged();
                        }
                    }
                    
                    // Notify.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ScaleDataViewModel ScaleData
        {
            get { return this.scaleDataViewModelValue; }
        }

        public LoadsEmptyViewModel LoadsEmpty
        {
            get { return this.loadsEmptyViewModelValue; }
        }

        #endregion

        #endregion
    }
}
