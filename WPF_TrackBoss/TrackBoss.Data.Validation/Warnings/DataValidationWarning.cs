using TrackBoss.Data.Validation.Enumerations;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.Warnings
{
    public abstract class DataValidationWarning : DataValidationEntity
    {
        /// <summary>
        /// Default constructor. Initializes fields and prepares this object 
        /// for use.
        /// </summary>
        public DataValidationWarning()
        {
            // Initialize fields.
            this.Severity = ValidationSeverity.Warning;
        }
    }
}
