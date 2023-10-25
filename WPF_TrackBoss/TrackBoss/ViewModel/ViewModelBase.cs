using System.ComponentModel;
using System.Runtime.CompilerServices;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel
{
    /// <summary>
    /// Implements base class for all view models.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Fields
        
        /// <summary>
        /// Indicates whether or not this object is currently selected.
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// The string representation of this object.
        /// </summary>
        private string displayText;

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Property changed event declaration.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event. 
        /// 
        /// NOTE: The attribute "CallerMemberName" substitutes the caller's 
        /// name for the propertyName by default.
        /// </summary>
        /// <param name="propertyName">The name of the property that has been
        /// changed.</param>
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "") 
        {
            // Raise property changed event.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets/sets the text that should be used as the representation of 
        /// this object.
        /// </summary>
        public string DisplayText
        {
            get { return this.displayText; }
            set
            {
                if (this.displayText != value)
                {
                    this.displayText = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets/sets a flag indicating whether or not this view model is 
        /// selected.
        /// </summary>
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
