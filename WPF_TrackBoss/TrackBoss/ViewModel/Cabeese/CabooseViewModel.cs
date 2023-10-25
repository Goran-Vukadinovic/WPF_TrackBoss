using System;
using System.Text;
using TrackBoss.Data;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.RollingStocks;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cabeese
{
    public class CabooseViewModel : ChangeTrackingViewModel, IDisposable
    {
        #region Fields
        
        private CabeeseDesignerViewModel designerViewModel;

        private Caboose caboose;

        private RollingStockViewModel rollingStockViewModelValue;

        private CupolaTypeViewModel CupolaTypeViewModelValue;

        private ScaleDataViewModel scaleDataViewModelValue;
        
        private bool disposed;

        #endregion

        #region Constructor(s)

        private CabooseViewModel()
        {
            // Hook-up commands.
        }

        public CabooseViewModel(Caboose Caboose) : this()
        {
            // Validate parameter.
            if (Caboose == null)
                throw new ArgumentNullException(nameof(Caboose));

            // Validate components. All of these are REQUIRED for a Caboose
            // object to be considered valid.
            if (Caboose.RollingStock == null)
                throw new InvalidOperationException("The rolling stock object is invalid.");
            if (Caboose.ScaleData == null)
                throw new InvalidOperationException("The scale data object is invalid.");

            // Assign member fields.
            this.caboose = Caboose;
            this.rollingStockViewModelValue = new RollingStockViewModel(this.caboose.RollingStock);
            this.scaleDataViewModelValue = new ScaleDataViewModel(this.caboose.ScaleData);
            if (this.caboose.CupolaType != null)
                this.CupolaTypeViewModelValue = new CupolaTypeViewModel(this.caboose.CupolaType);

            // Hook-up event handlers.
            this.rollingStockViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
            this.scaleDataViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;

            // Initialize properties to their defaults.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        public CabooseViewModel(CabeeseDesignerViewModel designerViewModel, Caboose Caboose) : this(Caboose)
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

                    // Dispose of child ViewModels that need disposing.
                    this.rollingStockViewModelValue.Dispose();
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
            this.DisplayText = this.caboose.ToString();
        }

        #endregion

        #region Command Handlers

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
        /// Returns the Caboose data object encapsulated by this ViewModel.
        /// </summary>
        /// <returns>The encapsulated Caboose data object.</returns>
        public Caboose ToCaboose()
        {
            return this.caboose;
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
        ///     "T" - Cupola Type
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
                        builder.Append(this.CupolaType != null ? this.CupolaType.DisplayText : null);
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

        public CabeeseDesignerViewModel Designer
        {
            get { return this.designerViewModel; }
        }

        public long ID
        {
            get { return this.caboose.ID; }
        }

        public string UniqueId
        {
            get { return this.caboose.UniqueId; }
        }

        #region Child ViewModels

        public RollingStockViewModel RollingStock
        {
            get { return this.rollingStockViewModelValue; }
        }

        public CupolaTypeViewModel CupolaType
        {
            get { return this.CupolaTypeViewModelValue; }
            set
            {
                // Don't allow null assignment.
                if (value == null)
                    return;

                if (this.CupolaTypeViewModelValue != value)
                {
                    // Set new type.
                    this.CupolaTypeViewModelValue = value;

                    // Assign new type.
                    this.caboose.CupolaType = this.CupolaTypeViewModelValue.ToCupolaType();

                    if (Designer.IsNotMultiSelection == false)
                    {
                        foreach (CabooseViewModel item in Designer.selectedViewModels)
                        {
                            // Set new type.
                            item.CupolaTypeViewModelValue = value;

                            // Assign new type.
                            item.caboose.CupolaType = this.CupolaTypeViewModelValue.ToCupolaType();
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

        #endregion

        #endregion
    }
}
