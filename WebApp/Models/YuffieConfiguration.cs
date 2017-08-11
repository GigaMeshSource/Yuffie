using System.Collections.Generic;

namespace Yuffie.WebApp.Models
{
    public class YuffieConfiguration
    {
        public List<YCPage> Pages {get;set;}
    }

    public class YCPage 
    {
        public string Name {get;set;}
        public List<YCPSection> Sections {get;set;}
    }
    public class YCPSection 
    {
        public string Name {get;set;}
        public List<YCPSElement> Elements {get;set;}        
    }

    public class YCPSElement
    {
        public string Name {get;set;}
        public string Type {get;set;}
        public List<string> Items {get;set;}
        public string TextType {get;set;}
        public string Default {get;set;}
        public List<YCPSElement> Elements {get;set;}
    }
}