//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace otr_project_internal.Models
{
    public partial class ItemImageFileModel
    {
        public string Id { get; set; }
        public string ItemModelId { get; set; }
        public string Name { get; set; }
        public byte[] Contents { get; set; }
        public string ContentType { get; set; }
    
        public virtual ItemModel ItemModel { get; set; }
    }
    
}
