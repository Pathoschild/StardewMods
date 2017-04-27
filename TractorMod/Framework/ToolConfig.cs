namespace TractorMod.Framework
{
    public class ToolConfig
    {
        /*********
        ** Properties
        *********/
        private int activeCount = 0;


        /*********
        ** Accessors
        *********/
        public string name { get; set; } = "";
        public int minLevel { get; set; } = 0;
        public int effectRadius { get; set; } = 1;
        public int activeEveryTickAmount { get; set; } = 1;


        /*********
        ** Public methods
        *********/
        public ToolConfig() { }

        public ToolConfig(string nameInput)
        {
            name = nameInput;
        }

        public ToolConfig(string nameInput, int minLevelInput, int radiusInput, int activeEveryTickAmountInput)
        {
            name = nameInput;
            minLevel = minLevelInput;
            effectRadius = radiusInput;
            if (activeEveryTickAmountInput <= 0)
                activeEveryTickAmount = 1;
            else
                activeEveryTickAmount = activeEveryTickAmountInput;
        }

        public void incrementActiveCount()
        {
            activeCount++;
            activeCount %= activeEveryTickAmount;
        }

        public bool canToolBeActive()
        {
            return (activeCount == 0);
        }
    }
}
