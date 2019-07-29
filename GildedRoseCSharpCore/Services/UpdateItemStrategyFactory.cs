using GildedRoseCSharpCore.Entity;
using GildedRoseCSharpCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GildedRoseCSharpCore.Services
{
    public class UpdateItemStrategyFactory : IUpdateItemStrategyFactory
    {   
        public void UpdateItem(ItemList item)
        {
            if(!item.Properties.IsLegendary)
            {
                item.SellIn--;

                if (item.SellIn < 0)
                    item.Quality = item.Properties.FinalQuality != null ? (int)item.Properties.FinalQuality : item.Quality + item.Properties.AfterSellIn;                    
                else if (item.Properties.SpecialConditions != null && item.Properties.SpecialConditions.Count > 0)
                    HandleSpecialConditions(item);
                else if (item.Quality >= item.Properties.LowerBound && item.Quality <= item.Properties.UpperBound)
                    item.Quality = item.Quality + item.Properties.QualityIncrOrDecr;
                
                item.Quality = Math.Max(item.Quality,item.Properties.LowerBound);
                item.Quality = Math.Min(item.Quality, item.Properties.UpperBound);
            }
        }

        // Method to handle the special conditions like backstage passes quality update based on sellIn value
        private void HandleSpecialConditions(ItemList item)
        {
            List<int> specialQuality = item.Properties.SpecialConditions.Select(x => x.SellIn - item.SellIn).ToList();
            specialQuality = specialQuality.Where(x => x >= 0).ToList();

            if (specialQuality.Count > 0)
            {
                var splCondition = item.Properties.SpecialConditions.FirstOrDefault(x => x.SellIn == item.SellIn + specialQuality.Min());
                item.Quality = item.Quality + splCondition.QualityIncrOrDecr;
            }
            else
                item.Quality = item.Quality + item.Properties.QualityIncrOrDecr;
        }
    }
}
