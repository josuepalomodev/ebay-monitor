using System.Globalization;
using System.Text.RegularExpressions;
using EbayMonitor.Server.Entities;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace EbayMonitor.Server.Controllers;

[ApiController]
[Route("[controller]")]
public partial class EbayListingsController
{
    private static readonly HttpClient Client = new();
    
    [HttpGet(Name = "GetEbayListings")]
    public async Task<IEnumerable<EbayListing>> Get()
    {
        var ebayListings = new List<EbayListing>();
        
        try
        {
            var testUri = new Uri("https://www.ebay.com/sch/i.html?_from=R40&_nkw=6700xt&_sacat=0&_sop=10&rt=nc&LH_BIN=1");
            var responseBody = await Client.GetStringAsync(testUri);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            var ebayListingNodes =
                htmlDocument.DocumentNode.SelectNodes(
                    ".//li[contains(@class,'s-item')]");
            
            foreach (var ebayListingNode in ebayListingNodes)
            {
                var id = ebayListingNode.Id;
                if (id.Length == 0)
                {
                    continue;
                }
                
                var title = 
                    ebayListingNode.SelectSingleNode(".//div[contains(@class, 's-item__title')]/span[contains(@role, 'heading')]").InnerText;
                
                var isNewListing = title.Contains("New Listing");
                if (isNewListing)
                {
                    title = title.Replace("New Listing", "");
                }
                
                var condition = ebayListingNode.SelectSingleNode(".//div[contains(@class, 's-item__subtitle')]/span[contains(@class, 'SECONDARY_INFO')]").InnerText;
                
                var itemPriceUsdRaw = ebayListingNode.SelectSingleNode(".//div[contains(@class, 's-item__detail')]/span[contains(@class, 's-item__price')]").InnerText;
                var shippingPriceUsdRaw = ebayListingNode
                    .SelectSingleNode(
                        ".//div[contains(@class, 's-item__detail')]/span[contains(@class, 's-item__shipping')]").InnerText;
                
                var usdRegex = MyRegex();
                var itemPriceUsdMatch = usdRegex.Match(itemPriceUsdRaw);
                var itemPriceUsd = double.Parse(itemPriceUsdMatch.Value);
                var shippingPriceUsdMatch = usdRegex.Match(shippingPriceUsdRaw ?? "");
                var shippingPriceUsd = shippingPriceUsdMatch.Success ? double.Parse(shippingPriceUsdMatch.Value) : 0;

                var listedAtRaw = ebayListingNode.SelectSingleNode(
                    ".//span[contains(@class, 's-item__detail')]//span[contains(@class, 's-item__listingDate')]").InnerText;
                
                var listedAt = DateTime.ParseExact(listedAtRaw, "MMM-d HH:mm", CultureInfo.InvariantCulture);

                var ebayListing = new EbayListing
                {
                    Id = id,
                    Title = title,
                    IsNewListing = isNewListing,
                    Condition = condition,
                    ItemPriceUsd = itemPriceUsd,
                    ShippingPriceUsd = shippingPriceUsd,
                    ListedAt = listedAt
                };
                ebayListings.Add(ebayListing);
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nAn exception occurred attempting to fetch ebay listings.");
            Console.WriteLine("Message :{0} ", e.Message);
        }
        
        return ebayListings;
    }

    [GeneratedRegex("\\d+(\\.\\d{2})?")]
    private static partial Regex MyRegex();
}