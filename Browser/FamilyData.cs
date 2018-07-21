using System;

namespace RevitFamilyBrowser.WPF_Classes
{
    public class FamilyData
    {
        //族名称
        public string FamilyName { get; set; }

        public string FullName { get; set; }
        //族模板
        public string Name { get; set; }

        public Uri img { get; set; }

        public Uri familyImage { get; set; }
    }
}
