/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data
{
    public partial class AliasPrinter
    {
        #region Validation Methods

        #endregion
        
        #region Object Overrides

        public override string ToString()
        {
            // Return alias as default, if present.
            if (!string.IsNullOrEmpty(this.Alias))
                return this.Alias;

            // Check printer object. Technically, no printer object indicates
            // an error condition, but proceed anyway.
            if (this.Printer != null)
            {
                // Return name or device name.
                if (!string.IsNullOrEmpty(this.Printer.Name))
                    return this.Printer.Name;
                else if (!string.IsNullOrEmpty(this.Printer.DeviceName))
                    return this.Printer.DeviceName;
            }

            // Return default.
            return base.ToString();
        }

        #endregion
    }
}
