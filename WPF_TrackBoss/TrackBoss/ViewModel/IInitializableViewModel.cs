using TrackBoss.Mvvm.Shared.Commands;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel
{
    public interface IInitializableViewModel
    {
        /// <summary>
        /// Performs initialization on this object.
        /// </summary>
        /// <returns>The command used to invoke initialization.</returns>
        IAsyncCommand InitializeCommand { get; }
    }
}
