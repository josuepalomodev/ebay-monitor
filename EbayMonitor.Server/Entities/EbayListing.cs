namespace EbayMonitor.Server.Entities;

public class EbayListing
{
    public string? Id { get; set; }
    
    public string? UrlLink { get; set; }

    public string? Title { get; set; }

    public bool IsNewListing { get; set; }

    public string? ImageUrl { get; set; }

    public string? Condition { get; set; }

    public double ItemPriceUsd { get; set; }
    
    public double ShippingPriceUsd { get; set; }

    public double SalesTaxUsd { get; set; }

    public double TotalPriceUsd { get; set; }

    public DateTime ListedAt { get; set; }
}