
using System.Collections.Generic;

namespace GildedRoseCSharpCore.Entity
{
    public class Properties : Item
    {
        public bool IsLegendary { get; set; }

        public int LowerBound { get; set; } = 0;

        public int UpperBound { get; set; } = 50;

        public int AfterSellIn { get; set; }

        public int? FinalQuality { get; set; }

        public int QualityIncrOrDecr { get; set; }

        public List<SpecialCondition> SpecialConditions { get; set; } = null;
    }
}
