using System;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.Warnings.Numeric
{
    public class SuggestedNumericDataPointWarning : DataValidationWarning
    {
        #region Fields

        #endregion

        #region Constructor(s)

        #endregion

        #region Override Methods

        /// <summary>
        /// Performs the actual validation.
        /// </summary>
        /// <param name="validationObject">The object to be used
        /// during the validation process.</param>
        public override bool Validate(object validationObject)
        {
            // If object is null, this is an automatic failure.
            if (validationObject == null)
                return false;

            // Cast object.
            double value;
            if (validationObject is int)
                value = (int)validationObject;
            else if (validationObject is long)
                value = (long)validationObject;
            else if (validationObject is double)
                value = (double)validationObject;
            else
                throw new NotSupportedException(string.Format("The specified numeric type {0} is not supported.", validationObject.GetType().Name));

            // Return pass/fail.
            return value > 0D;
        }

        #endregion

        #region Event Handlers

        #endregion

        #region Command Handlers

        #endregion

        #region Properties

        #endregion
    }
}
