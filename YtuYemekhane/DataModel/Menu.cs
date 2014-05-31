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
        public String Date{get;set;}
        public String main_lunch{get;set;}
        public String alt_lunch{get;set;}
        public String alt_dinner{get;set;}
        public String main_dinner { get; set; }
    }

   
}
