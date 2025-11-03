using System;

namespace ReciepApp.Models.Shared
{
    public class ItemBase
    {
        public string Name { get; set; } = "";
        public string Category {get; set; } = "";
        public double Quantity { get; set; } = 0;
        public string Unit { get; set; } = "";
        public string?ImageUrl {get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}

