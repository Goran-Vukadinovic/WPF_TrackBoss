using System;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation
{
    public class DataValidationResult<T> where T : class
    {
        #region Fields
        
        #endregion

        #region Constructor(s)
        
        /// <summary>
        /// Overload constructor. Initializes this object using the specified
        /// data.
        /// </summary>
        /// <param name="sourceObject">The object being validated and that 
        /// these results are associated with.</param>
        /// <param name="dataPoint">The data point this validation result
        /// applies to.</param>
        /// <param name="entity">The data validation entity that validated
        /// the source object.</param>
        public DataValidationResult(T sourceObject, string dataPoint, DataValidationEntity entity)
        {
            // Validate objects.
            if (sourceObject == null)
                throw new ArgumentNullException(nameof(sourceObject));
            if (string.IsNullOrWhiteSpace(dataPoint))
                throw new InvalidOperationException("The data point name is missing or invalid.");
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Assign properties.
            this.Source = sourceObject;
            this.DataPoint = dataPoint;
            this.Entity = entity;
        }

        #endregion
        
        #region Private Methods

        #endregion

        #region Event Handlers

        #endregion

        #region Command Handlers

        #endregion

        #region Properties
                
        /// <summary>
        /// Gets the data point this validation result applies to.
        /// </summary>
        public string DataPoint { get; private set; }

        /// <summary>
        /// Gets the object this result applies to.
        /// </summary>
        public T Source { get; private set; }
        
        /// <summary>
        /// Gets the data validation entity this result applies to.
        /// </summary>
        public DataValidationEntity Entity { get; private set; }

        #endregion
    }
}
