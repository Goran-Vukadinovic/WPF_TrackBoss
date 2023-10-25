using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Utilities
{
    public static class RollingStockDataHelper
    {
        /// <summary>
        /// Returns a list of rolling stock, filtered by dwell time data rules
        /// for the given track.
        /// </summary>
        /// <param name="track">The track to get the list of rolling stock
        /// from.</param>
        /// <returns>A list of available rolling stock on the given track.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<RollingStock> GetDwellTimeAdjustedRollingStockList(Track track, DwellTimeData trackDwellTimeData)
        {
            // Validate parameters.
            if (track == null)
                throw new ArgumentNullException(nameof(track));
            if (track.Location == null)
                throw new InvalidOperationException("The track's location is invalid.");
            if (trackDwellTimeData == null)
                throw new ArgumentNullException(nameof(trackDwellTimeData));

            // Iterate through attached statuses and get available cars.
            List<RollingStock> filteredRollingStock = new List<RollingStock>();
            foreach (RollingStockStatus status in track.Location.RollingStockStatuses)
            {
                // A null dwell time data object is something which SHOULD NOT
                // happen.
                if (status.DwellTimeData != null)
                {
                    // Make sure rolling stock is available.
                    if (status.DwellTimeData.Available.HasValue && status.DwellTimeData.Available.Value)
                    {
                        // IMPORTANT NOTE: Global dwell time does not exist as a separate
                        // entity anywhere. When either a rolling stock or a track is set
                        // to use the global dwell time, its use global flag is set. Then
                        // its current value, dwell time value, and dwell time method are
                        // set to whatever the global's is. So, the only place where the
                        // use global is being accounted for below is when the rolling
                        // stock's use global flag is set. In essence, global trumps every
                        // thing else.

                        // Check to see if rolling stock is using the global dwell time
                        // method and value.
                        if (status.DwellTimeData.UseGlobal.HasValue && status.DwellTimeData.UseGlobal.Value)
                        {
                            // Use car's.
                            if (status.DwellTimeData.CurrentValue == 0)
                                filteredRollingStock.Add(status.RollingStock);
                        }
                        else
                        {
                            // Use track's.
                            if (trackDwellTimeData.CurrentValue == 0)
                                filteredRollingStock.Add(status.RollingStock);
                        }
                    }
                }
                else
                    throw new InvalidOperationException(
                        string.Format("The dwell time data object is in an invalid state for {0}.", status.RollingStock.ToString()));
            }

            // Return rolling stock list.
            return filteredRollingStock;
        }
        
        /// <summary>
        /// Filters the specified rolling stock by limiting it to the roads
        /// specified.
        /// </summary>
        /// <param name="roads">The list of allowed roads.</param>
        /// <param name="rollingStockList">The rolling stock list to 
        /// filter.</param>
        /// <returns>The list of filtered rolling stock.</returns>
        public static IEnumerable<RollingStock> FilterRollingStockByCarRoad(IEnumerable<Road> roads, IEnumerable<RollingStock> rollingStockList)
        {
            // Validate parameters.
            if (roads == null)
                throw new ArgumentNullException(nameof(roads));
            if (rollingStockList == null)
                throw new ArgumentNullException(nameof(rollingStockList));

            // Make sure there is something to do.
            if (roads.Count() == 0 || rollingStockList.Count() == 0)
                return rollingStockList;

            // Filter list and return.
            return rollingStockList.Where(x => x.Road != null && roads.Contains(x.Road));
        }
        
        /// <summary>
        /// Filters the specified rolling stock by limiting it to the car types
        /// specified.
        /// </summary>
        /// <param name="carTypes">The list of allowed car types.</param>
        /// <param name="cars">The cars list to filter.</param>
        /// <returns>The list of filtered rolling stock.</returns>
        public static IEnumerable<Car> FilterRollingStockByCarType(IEnumerable<CarType> carTypes, IEnumerable<Car> cars)
        {
            // Validate parameters.
            if (carTypes == null)
                throw new ArgumentNullException(nameof(carTypes));
            if (cars == null)
                throw new ArgumentNullException(nameof(cars));

            // Make sure there is something to do.
            if (carTypes.Count() == 0 || cars.Count() == 0)
                return cars;

            // Filter list and return.
            return cars.Where(x => x.CarType != null && carTypes.Contains(x.CarType));
        }
    }
}
