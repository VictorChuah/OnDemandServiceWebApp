//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace fyptest
{
    using System;
    using System.Collections.Generic;
    
    public partial class Document
    {
        public int DocId { get; set; }
        public string file { get; set; }
        public string holder { get; set; }
    
        public virtual Provider Provider { get; set; }
    }
}