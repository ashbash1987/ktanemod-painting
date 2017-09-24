public sealed class ColorBlindSet
{
    public sealed class ColorSwap
    {
        public ColorSwap(ColorOption from, ColorOption to)
        {
            From = from;
            To = to;
        }

        public ColorOption From
        {
            get;
            private set;
        }

        public ColorOption To
        {
            get;
            private set;
        }
    }

    public ColorBlindSet(string name, params ColorSwap[] swaps)
    {
        Name = name;    
        Swaps = swaps;
    }

    public string Name
    {
        get;
        private set;
    }

    public ColorSwap[] Swaps
    {
        get;
        private set;
    }
}

