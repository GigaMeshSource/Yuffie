using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuffie.WebApp.Models {

    public class Data {
      public Dictionary<object, object> Items {get;set;}
    }
    public class Entity {
        public List<Data> Data {get;set;}
    }
}