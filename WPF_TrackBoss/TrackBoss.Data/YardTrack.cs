//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrackBoss.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class YardTrack
    {
        public static YardTrack Create(Yard yard, Track track)
        {
            return new
                YardTrack()
            {
                Yard = yard,
                Track = track,
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public YardTrack()
        {
            this.YardArrivalTracks = new HashSet<YardArrivalTrack>();
            this.YardBlockTracks = new HashSet<YardBlockTrack>();
            this.YardCabooseTracks = new HashSet<YardCabooseTrack>();
            this.YardDepartureTracks = new HashSet<YardDepartureTrack>();
            this.YardEngineServiceTracks = new HashSet<YardEngineServiceTrack>();
            this.YardInterchangeTracks = new HashSet<YardInterchangeTrack>();
            this.YardPassengerTracks = new HashSet<YardPassengerTrack>();
            this.YardStagingTracks = new HashSet<YardStagingTrack>();
            this.YardStorageTracks = new HashSet<YardStorageTrack>();
            this.YardThruTracks = new HashSet<YardThruTrack>();
            this.YardTrainAssignedTracks = new HashSet<YardTrainAssignedTrack>();
        }
    
        public long ID { get; set; }
    
        public virtual Track Track { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardArrivalTrack> YardArrivalTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardBlockTrack> YardBlockTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardCabooseTrack> YardCabooseTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardDepartureTrack> YardDepartureTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardEngineServiceTrack> YardEngineServiceTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardInterchangeTrack> YardInterchangeTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardPassengerTrack> YardPassengerTracks { get; set; }
        public virtual Yard Yard { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardStagingTrack> YardStagingTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardStorageTrack> YardStorageTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardThruTrack> YardThruTracks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YardTrainAssignedTrack> YardTrainAssignedTracks { get; set; }
    }
}