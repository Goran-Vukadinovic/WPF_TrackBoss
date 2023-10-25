using System;
using System.Text;
using System.Threading.Tasks;

/**
* Programmer: Chad R. Hearn
* Entities:   TrackBoss, LLC 
* Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
*/

namespace TrackBoss.Data
{
    public partial class Car
    {
        #region Static Factory Methods

        /// <summary>
        /// Creates a new instance of this object.
        /// </summary>
        /// <returns>A new instance of this object for use.</returns>
        public static Car Create()
        {
            Guid uniqueId = Guid.NewGuid();
            return new 
                Car()
                {
                    UniqueId = uniqueId.ToString(),
                    RollingStock = RollingStock.Create(),
                    CarType = null,
                    ScaleData = ScaleData.Create(),
                    LoadsEmpty = LoadsEmpty.Create(),
                };
        }

        /// <summary>
        /// Clones the specified car.
        /// </summary>
        /// <param name="source">Car to be cloned</param>
        /// <returns>A clone of the source car.</returns>
        public static async Task<Car> Clone(Car source)
        {
            // Validate source.
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Guid uniqueId = Guid.NewGuid();
            return new
                Car()
                {
                    UniqueId = uniqueId.ToString(),
                    RollingStock = await RollingStock.Clone(source.RollingStock),
                    CarType = source.CarType,
                    ScaleData = ScaleData.Clone(source.ScaleData),
                    LoadsEmpty = LoadsEmpty.Clone(source.LoadsEmpty),
                };
        }

        #endregion

        #region Object Overrides

        public override string ToString()
        {
            // Validate RollingStock.
            if (this.RollingStock == null)
                throw new InvalidOperationException("The rolling stock object is invalid.");
            
            // Use the rolling stock's method if this car has a number.
            if(!string.IsNullOrWhiteSpace(this.RollingStock.Number))
                return this.RollingStock.ToString();

            // Return default.
            return string.Format("Car ({0})", this.ID);
        }

        #endregion
    }
}
