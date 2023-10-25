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
    using TrackBoss.Data.Enumerations;

    public partial class Spur
    {
        public static Spur Create(City city)
        {
            Guid uniqueId = Guid.NewGuid();
            Spur newSpur = new
                Spur()
            {
                DirectionRestrictionEnum = 0,
                ScaleLength = 0,
                Track = Track.Create(TrackType.Spur),
                City = city,
            };

            newSpur.Track.Location.Parent = city.Site.Location;
            return newSpur;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Spur()
        {
            this.IndustryAssignedSpurs = new HashSet<IndustryAssignedSpur>();
            this.SpurAssignedServices = new HashSet<SpurAssignedService>();
        }
    
        public long ID { get; set; }
        public Nullable<long> PrintOrder { get; set; }
        public Nullable<long> ScaleLength { get; set; }
        public Nullable<long> DirectionRestrictionEnum { get; set; }
        public Nullable<long> ServicePriority { get; set; }
    
        public virtual City City { get; set; }
        public virtual DwellTimeData DwellTimeData { get; set; }
        public virtual Industry Industry { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IndustryAssignedSpur> IndustryAssignedSpurs { get; set; }
        public virtual KammFactor KammFactor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpurAssignedService> SpurAssignedServices { get; set; }
        public virtual SpurOffSpotPermission SpurOffSpotPermission { get; set; }
        public virtual Track Track { get; set; }
    }
}