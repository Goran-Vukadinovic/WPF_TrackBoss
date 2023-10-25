using System;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data
{
    public partial class YardTrack
    {
        #region Private Methods

        #endregion

        #region Public Methods

        ///// <summary>
        ///// Sets the yard for this track.
        ///// </summary>
        ///// <param name="yard">The yard that this yard track should be
        ///// associated with.</param>
        //public void SetYard(Yard yard)
        //{
        //    // Validate params.
        //    if (yard == null)
        //        throw new ArgumentNullException(nameof(yard));
        //    if (yard.Site == null || yard.Site.Location == null)
        //        throw new InvalidOperationException("Not all required components are present.");
        //    if (this.Track == null)
        //        throw new InvalidOperationException("The track is null.");
        //    if (this.Track.Location == null)
        //        throw new InvalidOperationException("The track has no location.");

        //    // Set yard and appropriate properties.
        //    this.Yard = yard;
        //    this.Track.Location.Parent = yard.Site.Location;
        //}

        #endregion

        #region Object Overrides

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <returns>The name of this track if it has data. If it doesn't, then
        /// the default fully qualified name is returned.</returns>
        public override string ToString()
        {
            // Make sure location is good.
            if (this.Track != null)
            {
                // Return name.
                if (!string.IsNullOrEmpty(this.Track.Location.Name))
                    return this.Track.Location.Name;
            }

            // Return default.
            return base.ToString();
        }

        #endregion
    }
}
