using System;
using System.Collections.Generic;
using TrackBoss.Data.Validation.Rules;
using TrackBoss.Data.Validation.Warnings;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.RuleSets
{
    public class DataPointValidationResult<T> where T : class
    {
        #region Fields

        private List<DataValidationRule> errorsValue;

        private List<DataValidationWarning> warningsValue;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Overload constructor. Initializes fields and prepares this object
        /// for use using the specified data.
        /// </summary>
        /// <param name="source">The source object being validated.</param>
        /// <param name="dataPointName">The name of the data point whose rules
        /// and warning are being evaluated for the object.</param>
        /// <param name="errors">The rules which should be evaluated for this
        /// data point.</param>
        /// <param name="warnings">The warning which should be evaluated for 
        /// this data point.</param>
        public DataPointValidationResult(T source, string dataPointName, List<DataValidationRule> errors, List<DataValidationWarning> warnings)
        {
            // Validate parameters.
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(dataPointName))
                throw new InvalidOperationException("The data point name is missing or invalid.");
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));
            if (warnings == null)
                throw new ArgumentNullException(nameof(warnings));

            // Assign.
            this.Source = source;
            this.Id = dataPointName;
            this.errorsValue = errors;
            this.warningsValue = warnings;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Event Handlers

        #endregion

        #region Command Handlers

        #endregion

        #region Properties

        public IReadOnlyList<DataValidationRule> Errors
        {
            get { return this.errorsValue; }
        }

        public IReadOnlyList<DataValidationWarning> Warnings
        {
            get { return this.warningsValue; }
        }

        /// <summary>
        /// Gets the data point ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the object these results apply to.
        /// </summary>
        public T Source { get; private set; }

        #endregion
    }
}
