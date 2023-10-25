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
    public partial class City : IEquatable<City>, IComparable
    {
        #region Static Factory Methods

        /// <summary>
        /// Creates a new instance of this object.
        /// </summary>
        /// <returns>A new instance of this object for use.</returns>
        public static City Create()
        {
            Guid uniqueId = Guid.NewGuid();
            return new
                City()
                {
                    UniqueId = uniqueId.ToString(),
                    Site = Site.Create(SiteType.City),
                };
        }

        /// <summary>
        /// Clones the specified city.
        /// </summary>
        /// <param name="source">City to be cloned</param>
        /// <returns>A clone of the source city.</returns>
        public static async Task<City> Clone(City source)
        {
            // Validate source.
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Guid uniqueId = Guid.NewGuid();
            return new
                City()
                {
                    UniqueId = uniqueId.ToString(),
                    Site = await Site.Clone(source.Site),
                    CityGroup = source.CityGroup,
                    Subdivision = source.Subdivision,
                    DivisionPoint = source.DivisionPoint,
                };
        }

        #endregion

        #region Operator Overrides

        public static int Compare(City left, City right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }
            if (object.ReferenceEquals(left, null))
            {
                return -1;
            }
            return left.CompareTo(right);
        }

        public static bool operator ==(City left, City right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(City left, City right)
        {
            return !(left == right);
        }

        public static bool operator <(City left, City right)
        {
            return (City.Compare(left, right) < 0);
        }

        public static bool operator >(City left, City right)
        {
            return (City.Compare(left, right) > 0);
        }

        #endregion

        #region Object Overrides

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <returns>The name of this city. If the required components are
        /// missing, the default fully qualified type name is 
        /// returned.</returns>
        public override string ToString()
        {
            // Make sure required components are present.
            if (this.Site == null || this.Site.Location == null)
                return base.ToString();

            // Return default.
            return this.Site.Location.Name;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as City);
        }

        public override int GetHashCode()
        {
            // This MUST BE unique across car types.
            return this.ID.GetHashCode();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(City other)
        {
            if (other == null)
                return false;
            return this.ID == other.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            City city = obj as City;
            if (city == null)
                return -1;

            return string.Compare(this.ToString(), city.ToString());
        }

        #endregion
    }
}
