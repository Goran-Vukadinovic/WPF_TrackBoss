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
    
    public partial class CarAvailability
    {
        public long ID { get; set; }
        public Nullable<bool> Immediate { get; set; }
    
        public virtual DwellTimeData DwellTimeData { get; set; }
    }
}
