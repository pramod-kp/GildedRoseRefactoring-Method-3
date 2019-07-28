using System;
using System.Collections.Generic;
using System.Text;

namespace GildedRoseCSharpCore.Entity
{
    public class Item
    {
        public string Name { get; set; }

        public int SellIn { get; set; }

        public int Quality { get; set; }

        public override string ToString()
        {
            return Name + ", " + SellIn + ", " + Quality;
        }
    }
}
