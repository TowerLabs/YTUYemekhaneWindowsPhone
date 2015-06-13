using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace YtuYemekhane.Model
{
    public class Menu
    {
        public String Date { get; set; }

        [JsonProperty("main_lunch")]
        public String MainLunch { get; set; }

        [JsonProperty("main_dinner")]
        public String MainDinner { get; set; }

        [JsonProperty("alt_lunch")]
        public String AltLunch { get; set; }

        [JsonProperty("alt_dinner")]
        public String AltDinner { get; set; }
    }
}
