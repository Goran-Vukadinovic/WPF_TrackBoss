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
    
    public partial class Work
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Work()
        {
            this.Stops = new HashSet<Stop>();
            this.WorkAssignedCarTypes = new HashSet<WorkAssignedCarType>();
            this.WorkAssignedRoads = new HashSet<WorkAssignedRoad>();
        }
    
        public long ID { get; set; }
        public Nullable<long> PointRestrictionEnum { get; set; }
        public Nullable<long> WorkTypeEnum { get; set; }
        public Nullable<long> LocalMoves { get; set; }
        public Nullable<long> MaxMoves { get; set; }
        public Nullable<long> MaxLength { get; set; }
        public Nullable<double> MaxWeight { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Stop> Stops { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WorkAssignedCarType> WorkAssignedCarTypes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WorkAssignedRoad> WorkAssignedRoads { get; set; }
    }
}
