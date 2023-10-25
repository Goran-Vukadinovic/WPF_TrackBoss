using System;
using System.Collections.Generic;
using TrackBoss.Data.Enumerations;
using TrackBoss.Data;
using TrackBoss.Shared.Extensions;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cars
{
    public class LoadsEmptyViewModel : ChangeTrackingViewModel
    {
        #region Fields

        private readonly SortedDictionary<CarStatusDisposition, string> statusDispositionsValue;

        private LoadsEmpty loadsEmpty;

        #endregion

        #region Constructor(s)

        private LoadsEmptyViewModel() : base()
        {
            // Load lists.
            this.statusDispositionsValue = EnumExtensions.GetDescriptionsDictionary<CarStatusDisposition>();
        }

        public LoadsEmptyViewModel(LoadsEmpty loadsEmpty) : this()
        {
            // Validate parameter.
            if (loadsEmpty == null)
                throw new ArgumentNullException(nameof(loadsEmpty));

            // Assign member fields.
            this.loadsEmpty = loadsEmpty;

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        #endregion

        #region Private Methods

        #endregion

        #region Event Handlers

        #endregion

        #region Public Methods

        #endregion

        #region Properties

        public SortedDictionary<CarStatusDisposition, string> StatusDispositions
        {
            get { return this.statusDispositionsValue; }
        }

        public CarStatusDisposition? StatusDisposition
        {
            get { return this.loadsEmpty.StatusDisposition; }
            set
            {
                if (this.loadsEmpty.StatusDisposition != value)
                {
                    this.loadsEmpty.StatusDisposition = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool Loaded
        {
            get
            {
                if (!this.loadsEmpty.Loaded.HasValue)
                    return false;
                return this.loadsEmpty.Loaded.Value;
            }
            set
            {
                if (this.loadsEmpty.Loaded != value)
                {
                    this.loadsEmpty.Loaded = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool RouteToHomeRoadOnEmpty
        {
            get
            {
                if (!this.loadsEmpty.RouteToHomeRoadOnEmpty.HasValue)
                    return false;
                return this.loadsEmpty.RouteToHomeRoadOnEmpty.Value;
            }
            set
            {
                if (this.loadsEmpty.RouteToHomeRoadOnEmpty != value)
                {
                    this.loadsEmpty.RouteToHomeRoadOnEmpty = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        
        public bool Use
        {
            get
            {
                if (!this.loadsEmpty.Use.HasValue)
                    return false;
                return this.loadsEmpty.Use.Value;
            }
            set
            {
                if (this.loadsEmpty.Use != value)
                {
                    this.loadsEmpty.Use = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
