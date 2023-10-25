using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.ViewModel.Cabeese;

namespace TrackBoss.ViewModel.Cities
{
    public class SpurIndustryViewModel
    {
        #region Constructors
        public SpurIndustryViewModel()
        {

        }
        #endregion

        public SpurViewModel spurViewModel;
        public IndustryViewModel industryViewModel;

        #region Properties
        private IconType _iconType = IconType.Unknown;
        public IconType iconType
        {
            get { return this._iconType; }
            set
            {
                _iconType = value;

                string iconRelativePath = "";
                if (_iconType == IconType.Industry)
                    iconRelativePath = "/Resources/Icons/Large/Black/industry.png";
                else if (_iconType == IconType.Spur)
                    iconRelativePath = "/Resources/Icons/Large/Black/track.png";
                else if (_iconType == IconType.General)
                    iconRelativePath = "/Resources/Icons/Large/Black/settings.png";
                else if (_iconType == IconType.PrintScheme)
                    iconRelativePath = "/Resources/Icons/Large/Black/print.png";

                // Return Uri of icon.
                ImageUri = new Uri(iconRelativePath, UriKind.Relative);
            }
        }

        private Uri _ImageUri;
        public Uri ImageUri
        {
            get { return this._ImageUri; }
            set
            {
                _ImageUri = value;
            }
        }

        private string displayText = "";
        public string DisplayText
        {
            get { return this.displayText; }
            set
            {
                if (this.displayText != value)
                {
                    this.displayText = value;
                }
            }
        }

        public string Name
        {
            get { return this.displayText; }
            set
            {
                if (this.displayText != value)
                {
                    this.displayText = value;
                }
            }
        }
        #endregion
    }
}
