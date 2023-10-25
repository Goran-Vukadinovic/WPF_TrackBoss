using System;
using System.Threading.Tasks;
using TrackBoss.Data.Enumerations;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data
{
    public partial class Caboose
    {
        #region Static Factory Methods

        /// <summary>
        /// Creates a new instance of this object.
        /// </summary>
        /// <returns>A new instance of this object for use.</returns>
        public static Caboose Create()
        {
            Guid uniqueId = Guid.NewGuid();
            Caboose newCaboose = new
                Caboose()
                {
                    UniqueId = uniqueId.ToString(),
                    RollingStock = RollingStock.Create(),
                    CupolaType = null,
                    ScaleData = ScaleData.Create(),
                };
            newCaboose.RollingStock.RollingStockType = RollingStockType.Caboose;
            return newCaboose;
        }

        /// <summary>
        /// Clones the specified caboose.
        /// </summary>
        /// <param name="source">Caboose to be cloned</param>
        /// <returns>A clone of the source caboose.</returns>
        public static async Task<Caboose> Clone(Caboose source)
        {
            // Validate source.
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Guid uniqueId = Guid.NewGuid();
            return new
                Caboose()
                {
                    UniqueId = uniqueId.ToString(),
                    RollingStock = await RollingStock.Clone(source.RollingStock),
                    CupolaType = source.CupolaType,
                    ScaleData = ScaleData.Clone(source.ScaleData),
                };
        }

        #endregion

        #region Validation Methods

        #endregion

        #region Object Overrides

        public override string ToString()
        {
            // Validate RollingStock.
            if (this.RollingStock != null)
                return this.RollingStock.ToString();

            // Return default.
            return base.ToString();
        }

        #endregion
    }
}
