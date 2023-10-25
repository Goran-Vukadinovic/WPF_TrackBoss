using TrackBoss.Data.Validation.Enumerations;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation
{
    public abstract class DataValidationEntity
    {
        #region Fields

        #endregion

        #region Constructor(s)

        #endregion

        #region Private Methods

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Performs the actual validation.
        /// </summary>
        /// <param name="validationObject">The object to be used
        /// during the validation process.</param>
        public abstract bool Validate(object validationObject);

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the severity for this result.
        /// </summary>
        public ValidationSeverity Severity { get; set; }

        /// <summary>
        /// Gets/sets the description for this rule. Note: this is the
        /// definition more or less of the rule.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets/sets the help text to be displayed for the user.
        /// </summary>                        
        public string HelpText { get; set; }

        #endregion
    }
}
