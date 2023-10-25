using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Utilities
{
    public static class DataHelper
    {
        /// <summary>
        /// Returns a list of type T where T is an entity from within the 
        /// database.
        /// </summary>
        /// <typeparam name="T">The type of the entity object to be
        /// returned.</typeparam>
        /// <param name="connection">The data context object to retrieve
        /// the list of objects from.</param>
        /// <returns>A "data set" of the given type from the data context
        /// object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<T> GetList<T>(DbContext connection) where T : class
        {
            // Validate parameter.
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            // Get corresponding set.
            DbSet<T> dbSet = connection.Set<T>();

            // Fetch list and return.
            return dbSet.ToList();
        }
    }
}
