using GildedRoseCSharpCore;
using GildedRoseCSharpCore.Entity;
using GildedRoseCSharpCore.Interfaces;
using GildedRoseCSharpCore.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace GildedRose.Tests
{
    public class GildedRoseTests
    {
        private IUpdateItemStrategyFactory _updateStrategy;
        public GildedRoseTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            IConfigurationRoot configuration = Helper.GetConfiguration();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            _updateStrategy = new UpdateItemStrategyFactory();
        }

        [Fact]
        public void shouldCheckDefaultOutput()
        {           
            var items = new List<ItemList>
            {
                new ItemList {Name = "+5 Dexterity Vest", SellIn = 10, Quality = 20,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = -1 } },
                new ItemList {Name = "Aged Brie", SellIn = 2, Quality = 0,
                    Properties = new Properties{ AfterSellIn = 1, QualityIncrOrDecr = 1, } },
                new ItemList {Name = "Elixir of the Mongoose", SellIn = 5, Quality = 7,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = -1 } },
                new ItemList {Name = "Sulfuras, Hand of Ragnaros", SellIn = 0, Quality = 80,
                    Properties = new Properties{ IsLegendary = true, AfterSellIn = 0, QualityIncrOrDecr = 0 } },
                new ItemList {
                    Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 15, Quality = 20,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = 1, FinalQuality = 0,
                        SpecialConditions = new List<SpecialCondition>()
                        {
                            new SpecialCondition { SellIn = 10, Operator="<=", QualityIncrOrDecr = 2 },
                            new SpecialCondition { SellIn = 5, Operator = "<=", QualityIncrOrDecr = 3 }
                        }
                    }
                },
                new ItemList {Name = "Conjured Mana Cake", SellIn = 3, Quality = 6,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = -2} }
            };

            var app = new Program(_updateStrategy, items);

            app.UpdateInventory();

            List<ItemList> expectedOutput = new List<ItemList>()
            {
                new ItemList { Name = "+5 Dexterity Vest", SellIn = 9, Quality = 19 },
                new ItemList { Name = "Aged Brie", SellIn = 1, Quality = 1 },
                new ItemList { Name = "Elixir of the Mongoose", SellIn = 4, Quality = 6 },
                new ItemList { Name = "Sulfuras, Hand of Ragnaros", SellIn = 0, Quality = 80 },
                new ItemList { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 14, Quality = 21 },
                new ItemList { Name = "Conjured Mana Cake", SellIn = 2, Quality = 4 }
            };

            Assert.Equal(expectedOutput[0].ToString(), app.Items[0].ToString());
            Assert.Equal(expectedOutput[1].ToString(), app.Items[1].ToString());
            Assert.Equal(expectedOutput[2].ToString(), app.Items[2].ToString());
            Assert.Equal(expectedOutput[3].ToString(), app.Items[3].ToString());
            Assert.Equal(expectedOutput[4].ToString(), app.Items[4].ToString());
            Assert.Equal(expectedOutput[5].ToString(), app.Items[5].ToString());
        }

        //- Once the sell by date has passed, Quality degrades twice as fast
        [Fact]
        public void shouldDecreasetheQualityTwiceIfSellByDateIsPassed()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList
                {
                    Name = "+5 Dexterity Vest", SellIn = 0, Quality = 20,
                    Properties = new Properties { AfterSellIn = -2, QualityIncrOrDecr = -1 }
                }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "+5 Dexterity Vest", SellIn = -1, Quality = 18 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- The Quality of an item is never negative
        [Fact]
        public void qualityOfItemShouldNotBeNegative()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList
                {
                    Name = "+5 Dexterity Vest", SellIn = 0, Quality = 0,
                    Properties = new Properties { AfterSellIn = -2, QualityIncrOrDecr = -1 }
                }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "+5 Dexterity Vest", SellIn = -1, Quality = 0 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- "Aged Brie" actually increases in Quality the older it gets
        [Fact]
        public void shouldincreasetheQualityof_AgedBrie()
        {
            List<ItemList> items = new List<ItemList>() { new ItemList {Name = "Aged Brie", SellIn = 3, Quality = 4,
                    Properties = new Properties{ AfterSellIn = 1, QualityIncrOrDecr = 1, } } };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Aged Brie", SellIn = 2, Quality = 5 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- The Quality of an item is never more than 50 except "Sulfuras", being a legendary item
        [Fact]
        public void qualityOfanItemShouldNotExceed50_ExceptSulfras()
        {
            List<ItemList> items = new List<ItemList>() { new ItemList {Name = "Aged Brie", SellIn = 2, Quality = 50,
                    Properties = new Properties{ AfterSellIn = 1, QualityIncrOrDecr = 1 } } };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Aged Brie", SellIn = 1, Quality = 50 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- "Sulfuras", being a legendary item, never has to be sold or decreases in Quality
        [Fact]
        public void shouldNotIncreaseOrDecreaseQualityof_Sulfuras()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {Name = "Sulfuras, Hand of Ragnaros", SellIn = 0, Quality = 80,
                    Properties = new Properties{ IsLegendary = true, AfterSellIn = 0, QualityIncrOrDecr = 0 } }
            };
            var app = new Program(_updateStrategy, items);            
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Sulfuras, Hand of Ragnaros", SellIn = 0, Quality = 80 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- "Backstage passes", like aged brie, increases in Quality as it's SellIn value approaches; Quality increases by 2 when there are 10 days or less 
        //  and by 3 when there are 5 days or less but Quality drops to 0 after the concert
        [Fact]
        public void shouldIncreaseInQualityBy1_IfMoreThan10Days_BackstagePasses()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {
                    Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 15, Quality = 20,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = 1, FinalQuality = 0,
                        SpecialConditions = new List<SpecialCondition>()
                        {
                            new SpecialCondition { SellIn = 10, Operator="<=", QualityIncrOrDecr = 2 },
                            new SpecialCondition { SellIn = 5, Operator = "<=", QualityIncrOrDecr = 3 }
                        }
                    }
                }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 14, Quality = 21 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        [Fact]
        public void shouldIncreaseInQualityBy2_IfLessThanOrEqualTo10Days_BackstagePasses()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {
                    Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 10, Quality = 20,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = 1, FinalQuality = 0,
                        SpecialConditions = new List<SpecialCondition>()
                        {
                            new SpecialCondition { SellIn = 10, Operator="<=", QualityIncrOrDecr = 2 },
                            new SpecialCondition { SellIn = 5, Operator = "<=", QualityIncrOrDecr = 3 }
                        }
                    }
                }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 9, Quality = 22 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        [Fact]
        public void shouldIncreaseInQualityBy3_IfLessThanOrEqualTo5Days_BackstagePasses()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {
                    Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 5, Quality = 20,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = 1, FinalQuality = 0,
                        SpecialConditions = new List<SpecialCondition>()
                        {
                            new SpecialCondition { SellIn = 10, Operator="<=", QualityIncrOrDecr = 2 },
                            new SpecialCondition { SellIn = 5, Operator = "<=", QualityIncrOrDecr = 3 }
                        }
                    }
                }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 4, Quality = 23 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        [Fact]
        public void shouldZerotheQualityAfterSellInDate_BackstagePasses()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {
                    Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 0, Quality = 20,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = 1, FinalQuality = 0,
                        SpecialConditions = new List<SpecialCondition>()
                        {
                            new SpecialCondition { SellIn = 10, Operator="<=", QualityIncrOrDecr = 2 },
                            new SpecialCondition { SellIn = 5, Operator = "<=", QualityIncrOrDecr = 3 }
                        }
                    }
                }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = -1, Quality = 0 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- The Quality of an item is never more than 50 except "Sulfuras", being a legendary item
        [Fact]
        public void qualityOfanItemShouldNotExceed50_BackstagePasses()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {
                    Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 4, Quality = 49,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = 1, FinalQuality = 0,
                        SpecialConditions = new List<SpecialCondition>()
                        {
                            new SpecialCondition { SellIn = 10, Operator="<=", QualityIncrOrDecr = 2 },
                            new SpecialCondition { SellIn = 5, Operator = "<=", QualityIncrOrDecr = 3 }
                        }
                    }
                }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 3, Quality = 50 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- "Conjured" items degrade in Quality twice as fast as normal items
        [Fact]
        public void shouldDecreaseQualityBy2_Conjured()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {Name = "Conjured Mana Cake", SellIn = 5, Quality = 30,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = -2} }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Conjured Mana Cake", SellIn = 4, Quality = 28 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }

        //- "Conjured" items degrade in Quality twice as fast as normal items
        [Fact]
        public void qualityOfShouldNotBeLessThan0_Conjured()
        {
            List<ItemList> items = new List<ItemList>()
            {
                new ItemList {Name = "Conjured Mana Cake", SellIn = 1, Quality = 1,
                    Properties = new Properties{ AfterSellIn = -2, QualityIncrOrDecr = -2} }
            };
            var app = new Program(_updateStrategy, items);
            app.UpdateInventory();
            List<ItemList> expectedOutput = new List<ItemList>() { new ItemList { Name = "Conjured Mana Cake", SellIn = 0, Quality = 0 } };

            Assert.Equal(expectedOutput[0].ToString(), items[0].ToString());
        }
    }
}
