namespace TractorMod.Framework
{
    public class ToolConfig
    {
        /*********
        ** Properties
        *********/
        private int activeCount;


        /*********
        ** Accessors
        *********/
        public string name { get; set; } = "";
        public int minLevel { get; set; }
        public int effectRadius { get; set; } = 1;
        public int activeEveryTickAmount { get; set; } = 1;


        /*********
        ** Public methods
        *********/
        public ToolConfig() { } // needed to read from config.json

        public ToolConfig(string nameInput)
        {
            name = nameInput;
        }

        public ToolConfig(string nameInput, int minLevelInput, int radiusInput, int activeEveryTickAmountInput)
        {
            name = nameInput;
            minLevel = minLevelInput;
            effectRadius = radiusInput;
            this.activeEveryTickAmount = activeEveryTickAmountInput > 0
                ? activeEveryTickAmountInput
                : 1;
        }

        public void incrementActiveCount()
        {
            activeCount++;
            activeCount %= activeEveryTickAmount;
        }

        public bool canToolBeActive()
        {
            return this.activeCount == 0;
        }
    }
}
