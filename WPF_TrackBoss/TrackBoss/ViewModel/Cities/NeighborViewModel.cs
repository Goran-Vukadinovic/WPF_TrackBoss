using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.ViewModel.Cabeese;

namespace TrackBoss.ViewModel.Cities
{
    public class NeighborViewModel : ChangeTrackingViewModel
    {
        private City city;

        #region Constructors
        public NeighborViewModel(City city)
        {
            this.city = city;
        }
        #endregion

        #region Constructors
        public City ToCity()
        {
            return city;
        }
        #endregion

        #region Properties
        public Site Site
        {
            get { return this.city.Site; }
        }
        #endregion
    }
}
