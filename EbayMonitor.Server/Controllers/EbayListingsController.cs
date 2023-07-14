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
    public async Task<IEnumerable<EbayListing>> Get([FromQuery] string? searchQuery, [FromQuery] string? positiveKeywords, 
        [FromQuery] string? negativeKeywords, [FromQuery] string? minTotalPrice, [FromQuery] string? maxTotalPrice,
        [FromQuery] string? dateFrom, [FromQuery] string? dateTo, [FromQuery] string? salesTaxRateUsd, [FromQuery] string? sort)
    {
        Console.WriteLine(salesTaxRateUsd);
        searchQuery = searchQuery?.ToLower();
        positiveKeywords = positiveKeywords?.ToLower();
        negativeKeywords = negativeKeywords?.ToLower();
        var ebayListings = new List<EbayListing>();
        
        try
        {
            var testUri = new Uri($"https://www.ebay.com/sch/i.html?_from=R40&_nkw={searchQuery}&_sacat=0&_sop=10&rt=nc&LH_BIN=1&_ipg=240");
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
                
                var listedAtRaw = ebayListingNode.SelectSingleNode(
                    ".//span[contains(@class, 's-item__detail')]//span[contains(@class, 's-item__listingDate')]").InnerText;
                
                var listedAt = DateTime.ParseExact(listedAtRaw, "MMM-d HH:mm", CultureInfo.InvariantCulture);

                if (!IsListedAtWithinRange(listedAt, dateFrom, dateTo))
                {
                    continue;
                }

                var urlLink = ebayListingNode
                    .SelectSingleNode(".//div[contains(@class, 's-item__info')]/a[contains(@class, 's-item__link')]")
                    .GetAttributeValue("href", "");

                var title =
                    ebayListingNode
                        .SelectSingleNode(".//div[contains(@class, 's-item__title')]/span[contains(@role, 'heading')]")
                        .InnerText;

                if (negativeKeywords != null && negativeKeywords.Split()
                        .Any(negativeKeyword => title.ToLower().Contains(negativeKeyword)))
                {
                    Console.WriteLine("1");
                    continue;
                }

                if (positiveKeywords != null && !positiveKeywords.Split()
                        .All(positiveKeyword => title.ToLower().Contains(positiveKeyword)))
                {
                    Console.WriteLine("2");
                    continue;
                }
                
                var isNewListing = title.Contains("New Listing");
                if (isNewListing)
                {
                    title = title.Replace("New Listing", "");
                }

                var imageUrl = ebayListingNode.SelectSingleNode(".//div[contains(@class, 's-item__image')]//img")
                    .GetAttributeValue("src", "");

                var condition =
                    ebayListingNode
                        .SelectSingleNode(
                            ".//div[contains(@class, 's-item__subtitle')]/span[contains(@class, 'SECONDARY_INFO')]")
                        ?.InnerText ?? "NA";

                var itemPriceUsdRaw = ebayListingNode
                    .SelectSingleNode(
                        ".//div[contains(@class, 's-item__detail')]/span[contains(@class, 's-item__price')]").InnerText;
                var shippingPriceUsdRaw = ebayListingNode
                    .SelectSingleNode(
                        ".//div[contains(@class, 's-item__detail')]/span[contains(@class, 's-item__shipping')]")
                    ?.InnerText;

                var usdRegex = MyRegex();
                var itemPriceUsdMatch = usdRegex.Match(itemPriceUsdRaw);

                double itemPriceUsd;
                try
                {
                    itemPriceUsd = double.Parse(itemPriceUsdMatch.Value);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("\nAn error occurred attempting to parse an ebay listing's item price.");
                    Console.WriteLine("Message :{0} ", e.Message);
                    continue;
                }

                var shippingPriceUsdMatch = usdRegex.Match(shippingPriceUsdRaw ?? "");
                var shippingPriceUsd = shippingPriceUsdMatch.Success ? double.Parse(shippingPriceUsdMatch.Value) : 0;

                var salesTaxUsd = itemPriceUsd * double.Parse(salesTaxRateUsd ?? "0");
                
                var totalPriceUsd = itemPriceUsd + shippingPriceUsd + salesTaxUsd;
                
                if (minTotalPrice != null && totalPriceUsd < double.Parse(minTotalPrice) || maxTotalPrice != null && totalPriceUsd > double.Parse(maxTotalPrice))
                {
                    continue;
                }

                var ebayListing = new EbayListing
                {
                    Id = id,
                    UrlLink = urlLink,
                    Title = title,
                    IsNewListing = isNewListing,
                    ImageUrl = imageUrl,
                    Condition = condition,
                    ItemPriceUsd = Math.Truncate(itemPriceUsd * 100) / 100,
                    ShippingPriceUsd = Math.Truncate(shippingPriceUsd * 100) / 100,
                    SalesTaxUsd = Math.Truncate(salesTaxUsd * 100) / 100,
                    TotalPriceUsd = Math.Truncate(totalPriceUsd * 100) / 100,
                    ListedAt = listedAt
                };
                ebayListings.Add(ebayListing);
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nAn error occurred attempting to fetch ebay listings.");
            Console.WriteLine("Message :{0} ", e.Message);
        }

        if (sort != null)
        {
            ebayListings.Sort((ebayListing1, ebayListing2) =>
            {
                var totalPrice1 = ebayListing1.ItemPriceUsd + ebayListing1.ShippingPriceUsd;
                var totalPrice2 = ebayListing2.ItemPriceUsd + ebayListing2.ShippingPriceUsd;

                if (sort == "totalPriceAsc")
                {
                    return totalPrice1.CompareTo(totalPrice2);
                }

                if (sort == "totalPriceDesc")
                {
                    return totalPrice2.CompareTo(totalPrice1);
                }

                if (sort == "newest")
                {
                    return ebayListing2.ListedAt.CompareTo(ebayListing1.ListedAt);
                }
                
                if (sort == "oldest")
                {
                    return ebayListing1.ListedAt.CompareTo(ebayListing2.ListedAt);
                }

                return 0;
            });
        }

        Console.WriteLine("Returned {0} eBayListings", ebayListings.Count);
        return ebayListings;
    }

    [GeneratedRegex("\\d+(\\.\\d{2})?")]
    private static partial Regex MyRegex();
    
    private static bool IsListedAtWithinRange(DateTime listedAt, string? dateFrom, string? dateTo)
    {
        if (string.IsNullOrEmpty(dateFrom) && string.IsNullOrEmpty(dateTo))
        {
            return true;
        }

        DateTime parsedDateFrom = DateTime.MinValue;
        DateTime parsedDateTo = DateTime.MaxValue;

        if (!string.IsNullOrEmpty(dateFrom) && !DateTime.TryParse(dateFrom, out parsedDateFrom))
        {
            throw new ArgumentException("Failed to parse dateFrom. Please ensure it's in the format 'MM/DD/YYYY'.");
        }

        if (!string.IsNullOrEmpty(dateTo) && !DateTime.TryParse(dateTo, out parsedDateTo))
        {
            throw new ArgumentException("Failed to parse dateTo. Please ensure it's in the format 'MM/DD/YYYY'.");
        }

        if (parsedDateTo == DateTime.Today && !(parsedDateFrom == parsedDateTo))
        {
            parsedDateTo = parsedDateTo.Date.AddDays(1).AddMinutes(-1);
        }

        if (parsedDateFrom == parsedDateTo)
        {
            parsedDateFrom = parsedDateFrom.Date;
            parsedDateTo = parsedDateTo.Date.AddDays(1).AddMinutes(-1);
        }

        
        return listedAt >= parsedDateFrom && listedAt <= parsedDateTo;
    }
}