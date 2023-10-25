using System.Collections.Generic;
using System.Xml.Serialization;
using TrackBoss.Data.Validation.Enumerations;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data.Validation.RuleSets
{
    public class RuleSet
    {
        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        public RuleSet()
        {
            // Initialize fields.
            this.DataPoints = new List<DataPoint>();
        }

        /// <summary>
        /// Gets/sets the type this rule set applies to.
        /// </summary>
        [XmlAttribute]
        public AppliesToType AppliesToType { get; set; }

        /// <summary>
        /// Gets/sets the version of the rule set.
        /// </summary>
        [XmlAttribute]
        public double Version { get; set; }

        /// <summary>
        /// Gets/sets a list of validation data points for this rule set.
        /// </summary>
        public List<DataPoint> DataPoints { get; set; }
    }
}
