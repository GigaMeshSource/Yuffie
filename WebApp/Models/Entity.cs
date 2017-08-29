using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuffie.WebApp.Models {

    public class Data {
        public int Id {get;set;}
        public string Key {get;set;}
        public object Value {get;set;}
    }
    public class Entity {
        public int Id {get;set;}
        public List<Data> Data {get;set;}
    }
}