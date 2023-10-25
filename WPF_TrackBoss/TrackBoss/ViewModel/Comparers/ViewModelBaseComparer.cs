using System.Collections;
using TrackBoss.Shared.Comparers;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Comparers
{
    /// <summary>
    /// Defines behaviors and properties of an object which can
    /// compare ViewModels.
    /// </summary>
    public class ViewModelBaseComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            // Get objects as correct types.
            ViewModelBase xViewModel = x as ViewModelBase;
            ViewModelBase yViewModel = y as ViewModelBase;

            // Perform null checks.
            if (xViewModel == null && yViewModel == null)
                return 0;
            else if (xViewModel == null)
                return 1;
            else if (yViewModel == null)
                return -1;

            // Perform object comparison.
            if (xViewModel == yViewModel)
                return 0;

            // Return default comparison.
            return AlphanumericComparer.CompareAsAlphaNum(xViewModel.DisplayText, yViewModel.DisplayText);
        }
    }
}
