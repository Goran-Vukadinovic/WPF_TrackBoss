using System.Collections.Generic;
using System.Xml.Serialization;
using TrackBoss.Data.Validation.Enumerations;
using TrackBoss.Data.Validation.Rules;
using TrackBoss.Data.Validation.Rules.Numeric;
using TrackBoss.Data.Validation.Rules.String;
using TrackBoss.Data.Validation.Warnings;
using TrackBoss.Data.Validation.Warnings.Numeric;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.RuleSets
{
    public class DataPoint
    {
        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        public DataPoint()
        {
            // Initialize fields.
            this.Rules = new List<DataValidationRule>();
            this.Warnings = new List<DataValidationWarning>();
        }

        /// <summary>
        /// Gets/sets the unique ID for this data point.
        /// </summary>
        [XmlAttribute]
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the type this data point should be treated as.
        /// </summary>
        [XmlAttribute]
        public DataPointType Type { get; set; }

        /// <summary>
        /// Gets/sets a list of validation rules for this rule set.
        /// </summary>
        [XmlArrayItem(nameof(RequiredNumericDataPointRule), typeof(RequiredNumericDataPointRule))]
        [XmlArrayItem(nameof(FloatingPointNumericRangeRule), typeof(FloatingPointNumericRangeRule))]
        [XmlArrayItem(nameof(IntegerNumericRangeRule), typeof(IntegerNumericRangeRule))]
        [XmlArrayItem(nameof(StringDataFormatRule), typeof(StringDataFormatRule))]
        [XmlArrayItem(nameof(StringMaxLengthRule), typeof(StringMaxLengthRule))]
        [XmlArrayItem(nameof(RequiredStringDataPointRule), typeof(RequiredStringDataPointRule))]
        public List<DataValidationRule> Rules { get; set; }

        /// <summary>
        /// Gets/sets a list of warnings for this rule set.
        /// </summary>
        [XmlArrayItem(nameof(SuggestedNumericDataPointWarning), typeof(SuggestedNumericDataPointWarning))]
        [XmlArrayItem(nameof(DataValidationWarning), typeof(DataValidationWarning))]
        public List<DataValidationWarning> Warnings { get; set; }
    }
}
