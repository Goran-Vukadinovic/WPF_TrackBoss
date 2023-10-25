/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data
{
    public partial class YardTrainAssignedTrack
    {
        #region Object Overrides

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <returns>The name of this track if it has data. If it doesn't, then
        /// the default fully qualified name is returned.</returns>
        public override string ToString()
        {
            // Make sure location is good.
            if (this.YardTrack != null)
                return this.YardTrack.ToString();

            // Return default.
            return base.ToString();
        }

        #endregion
    }
}
