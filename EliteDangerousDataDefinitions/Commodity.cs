namespace EliteDangerousDataDefinitions
{
    public class Commodity
    {
        // Definition of the commodity
        public string Name { get; set; }
        public string Category { get; set; }
        public long AveragePrice { get; set; }
        public bool Rare { get; set; }

        // Admin
        public long EDDBID { get; set; }

        public Commodity() { }

        public Commodity(Commodity Commodity)
        {
            this.EDDBID = Commodity.EDDBID;
            this.Name = Commodity.Name;
            this.Category = Commodity.Category;
            this.AveragePrice = Commodity.AveragePrice;
            this.Rare = Commodity.Rare;
        }

        public Commodity(long EDDBID, string Name, string Category, long AveragePrice, bool Rare)
        {
            this.EDDBID = EDDBID;
            this.Name = Name;
            this.Category = Category;
            this.AveragePrice = AveragePrice;
            this.Rare = Rare;
        }
    }
}
