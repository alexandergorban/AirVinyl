using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AirVinyl.Model
{
    public class DynamicProperty
    {
        [Key]
        [Column(Order = 1)]
        public string Key { get; set; }
        public string SerializedValue { get; set; }

        // EF can't store object values, so we need to work
        // with an in-between property to/from JSON representation
        public object Value
        {
            get { return JsonConvert.DeserializeObject(SerializedValue); }
            set { SerializedValue = JsonConvert.SerializeObject(value); }
        }

        [Key]
        [Column(Order = 2)]
        public int VinylRecordId { get; set; }
        public virtual VinylRecord VinylRecord { get; set; }
    }
}
