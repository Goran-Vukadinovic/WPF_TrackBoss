﻿using System.Collections;
using TrackBoss.Shared.Comparers;
using TrackBoss.ViewModel.Locomotives;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Comparers
{
    /// <summary>
    /// Defines behaviors and properties of an object which can
    /// compare LocomotiveViewModels and sort them in "reverse."
    /// </summary>
    public class LocomotiveViewModelReverseComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            // Get objects as correct types.
            LocomotiveViewModel xViewModel = x as LocomotiveViewModel;
            LocomotiveViewModel yViewModel = y as LocomotiveViewModel;

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

            // Return reverse of default comparison.
            int result = AlphanumericComparer.CompareAsAlphaNum(xViewModel.RollingStock.Number, yViewModel.RollingStock.Number);
            if (result == 1)
                return -1;
            else if (result == -1)
                return 1;
            return 0;
        }
    }
}
