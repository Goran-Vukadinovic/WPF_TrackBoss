using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TrackBoss.Data.Validation.Rules;
using TrackBoss.Data.Validation.RuleSets;
using TrackBoss.Data.Validation.Warnings;
using TrackBoss.Shared.Utilities.XML;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation
{
    public class Validator<T> where T : class
    {
        #region Fields

        private readonly RuleSet ruleSet;

        private List<DataValidationRule> validationResultsValue;

        private List<DataValidationWarning> validationWarningResultsValue;

        #endregion

        #region Constructor(s)
        
        /// <summary>
        /// Overload constructor. Initializes fields and prepares this object
        /// for use using the specified rule set.
        /// </summary>
        /// <param name="ruleSetFullPath">Full path to the rule set file to be
        /// used by this validator.</param>
        public Validator(string ruleSetFullPath)
        {
            // Validate full path.
            if (string.IsNullOrWhiteSpace(ruleSetFullPath))
                throw new InvalidOperationException("The full path for the rule set is missing or invalid.");
            if (!File.Exists(ruleSetFullPath))
                throw new InvalidOperationException("The specified rule set file does not exist.");

            // Load rule set.
            string contents = File.ReadAllText(ruleSetFullPath);
            this.ruleSet = GenericSerializer<RuleSet>.Deserialize(contents);

            // Prepare fields.
            this.validationResultsValue = new List<DataValidationRule>();
            this.validationWarningResultsValue = new List<DataValidationWarning>();
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Attempts to locate the specified property on the source object.
        /// </summary>
        /// <param name="sourceObject">The object to be queried for the
        /// specified property.</param>
        /// <param name="propertyName">The name of the property to look
        /// for.</param>
        /// <returns>The value of the property whose name is 
        /// specified.</returns>
        private object getPropertyParentObject(object sourceObject, string propertyName)
        {
            // Get properties on the object.
            Type validationObjectType = sourceObject.GetType();
            PropertyInfo[] propertyInfos = validationObjectType.GetProperties();

            // Locate the one with the name specified - i.e. the one that
            // corresponds with the data point's name.
            PropertyInfo dataPointPropertyInfo = propertyInfos.FirstOrDefault(x => x.Name == propertyName);
            if (dataPointPropertyInfo == null)
                throw new InvalidOperationException("The specified property does not exist on the validation object.");

            // Return value (which can be another object).
            return dataPointPropertyInfo.GetValue(sourceObject);
        }

        /// <summary>
        /// Attempts to retrieve the actual value of the data point from the
        /// specified object.
        /// </summary>
        /// <param name="sourceObject">The object to be used.</param>
        /// <param name="dataPointName">The name of the data point.</param>
        /// <returns>The value of the data point on the object.</returns>
        private object getDataPointValueFromValidationObject(T sourceObject, string dataPointName)
        {
            // Parse property (data point) name.
            string[] path = dataPointName.Split('.');
            if (path.Length < 1)
                throw new InvalidOperationException("The specified data point is invalid.");

            // Iterate through components and locate the requested property.
            object propertyParentObject = sourceObject;
            string propertyName = path[path.Length - 1];
            for(int i = 0; i < path.Length - 1; i++)
                propertyParentObject = this.getPropertyParentObject(propertyParentObject, path[i]);

            // Get properties on the object.
            Type validationObjectType = propertyParentObject.GetType();
            PropertyInfo[] propertyInfos = validationObjectType.GetProperties();

            // Locate the one with the name specified - i.e. the one that
            // corresponds with the data point's name.
            PropertyInfo dataPointPropertyInfo = propertyInfos.FirstOrDefault(x => x.Name == propertyName);
            if (dataPointPropertyInfo == null)
                throw new InvalidOperationException("The specified property does not exist on the validation object.");

            // Return the property's value.
            return dataPointPropertyInfo.GetValue(propertyParentObject);
        }

        #endregion

        #region Event Handlers

        #endregion

        #region Command Handlers

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears previous validation results and warnings.
        /// </summary>
        public void ClearValidationResults()
        {
            // Remove previous validation results.
            this.validationResultsValue.Clear();

            // Remove previous validation warnings.
            this.validationWarningResultsValue.Clear();
        }

        /// <summary>
        /// Validates all data points on the specified object.
        /// </summary>
        /// <param name="validationObject">The object whose data points should 
        /// be validated.</param>
        /// <returns>true if the object fails any validation checks, otherwise
        /// false.</returns>
        public bool Validate(T validationObject)
        {
            // Validate object.
            if (validationObject == null)
                throw new ArgumentNullException(nameof(validationObject));

            // Default to pass.
            bool result = true;

            // Iterate through data points and hand off processing to
            // sibling method.
            foreach(DataPoint dataPoint in this.ruleSet.DataPoints)
                result &= this.Validate(validationObject, dataPoint.Id);

            // Return result.
            return result;
        }

        /// <summary>
        /// Validates the data point of the specified object.
        /// </summary>
        /// <param name="validationObject">The object whose data point should be
        /// validated.</param>
        /// <param name="dataPointName">The name of the data point to be 
        /// validated.</param>
        /// <returns>true if the data point passes validation, otherwise 
        /// false.</returns>
        public bool Validate(T validationObject, string dataPointName)
        {
            // Validate object and data point name.
            if (validationObject == null)
                throw new ArgumentNullException(nameof(validationObject));
            if (string.IsNullOrWhiteSpace(dataPointName))
                throw new InvalidOperationException("The data point name is missing or invalid.");
            
            // Attempt to acquire data point that needs to be validated.
            DataPoint dataPoint = this.ruleSet.DataPoints.FirstOrDefault(x => x.Id == dataPointName);
            if (dataPoint == null)
                throw new InvalidOperationException("The specified data point is not present in the rule set.");
            
            // Validate rules.
            bool validationResult = true;
            foreach (DataValidationRule rule in dataPoint.Rules)
            {
                object dataPointValue = this.getDataPointValueFromValidationObject(validationObject, dataPointName);
                if (!rule.Validate(dataPointValue))
                {
                    validationResult = false;
                    this.validationResultsValue.Add(rule);
                }
            }

            // Execute warning checks.
            foreach(DataValidationWarning warning in dataPoint.Warnings)
            {
                if (!warning.Check(validationObject))
                    this.validationWarningResultsValue.Add(warning);
            }

            // Return result.
            return validationResult;
        }

        #endregion

        #region Properties

        public List<DataValidationRule> ValidationResults
        {
            get { return this.validationResultsValue; }
        }

        public List<DataValidationWarning> ValidationWarningResults
        {
            get { return this.validationWarningResultsValue; }
        }

        #endregion
    }
}
