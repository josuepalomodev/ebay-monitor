import { useState } from "react";
import { EbayListing } from "./types.ts";
import format from "date-fns/format";
import { parseISO } from "date-fns";

export default function App() {
  const [ebayListings, setEbayListings] = useState<EbayListing[]>([]);
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [keywords, setKeywords] = useState<string>("");
  const [totalPriceRange, setTotalPriceRange] = useState<[string, string]>([
    "",
    "",
  ]);
  const [sortBy, setSortBy] = useState<string>("totalPriceAsc");
  const [salesTaxRateUsd, setSalesTaxRate] = useState<string>("");
  const [dateRange, setDateRange] = useState<[string, string]>(["", ""]);

  async function fetchEbayListings(): Promise<void> {
    const [positiveKeywords, negativeKeywords] = parseKeywords(keywords);

    const serverApiEndpointTest = `https://localhost:44313/EbayListings?searchQuery=${searchQuery}${
      negativeKeywords ? `&negativeKeywords=${negativeKeywords}` : ""
    }${positiveKeywords ? `&positiveKeywords=${positiveKeywords}` : ""}${
      isNaN(Number(totalPriceRange[0])) || totalPriceRange[0] === ""
        ? ""
        : `&minTotalPrice=${Number(totalPriceRange[0])}`
    }${
      isNaN(Number(totalPriceRange[1])) || totalPriceRange[1] === ""
        ? ""
        : `&maxTotalPrice=${Number(totalPriceRange[1])}`
    }${dateRange[0] === "" ? "" : `&dateFrom=${dateRange[0]}`}${
      dateRange[1] === "" ? "" : `&dateTo=${dateRange[1]}`
    }${
      isNaN(Number(salesTaxRateUsd)) || salesTaxRateUsd === ""
        ? ""
        : `&salesTaxRateUsd=${Number(salesTaxRateUsd)}`
    }&sort=${sortBy}`;
    try {
      const response = await fetch(serverApiEndpointTest);
      if (response.ok) {
        setEbayListings((await response.json()) as EbayListing[]);
      }
    } catch (e) {
      console.log(e);
    }
  }

  function parseKeywords(keywordString: string): [string, string] {
    const keywordList: string[] = keywordString.split(",");
    const positiveKeywords: string[] = [];
    const negativeKeywords: string[] = [];

    for (const keyword of keywordList) {
      if (keyword.startsWith("+")) {
        positiveKeywords.push(keyword.slice(1));
      } else if (keyword.startsWith("-")) {
        negativeKeywords.push(keyword.slice(1));
      }
    }

    return [positiveKeywords.join("+"), negativeKeywords.join("+")];
  }

  return (
    <div className="min-h-screen max-w-screen text-base text-neutral-300 bg-neutral-900">
      <div className="px-4 my-4">
        <h4 className="text-lg">Task Manager</h4>
      </div>
      <div className="px-4 mb-2">
        <div className="space-y-2 mb-8">
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="searchQueryInput">
              Query
            </label>
            <input
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="searchQueryInput"
              type="text"
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="keywordsInput">
              Keywords
            </label>
            <input
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="keywordsInput"
              type="text"
              onChange={(e) => setKeywords(e.target.value)}
            />
          </div>
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="minItemPrice">
              Min Total Price
            </label>
            <input
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="minItemPrice"
              type="text"
              onChange={(e) =>
                setTotalPriceRange([e.target.value, totalPriceRange[1]])
              }
            />
          </div>
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="maxItemPrice">
              Max Total Price
            </label>
            <input
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="maxItemPrice"
              type="text"
              onChange={(e) =>
                setTotalPriceRange([totalPriceRange[0], e.target.value])
              }
            />
          </div>
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="dateFrom">
              Date From
            </label>
            <input
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="dateFrom"
              type="text"
              onChange={(e) => setDateRange([e.target.value, dateRange[1]])}
            />
          </div>
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="dateTo">
              Date To
            </label>
            <input
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="dateTo"
              type="text"
              onChange={(e) => setDateRange([dateRange[0], e.target.value])}
            />
          </div>
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="salesTaxRateUsd">
              Sales Tax Rate
            </label>
            <input
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="salesTaxRateUsd"
              type="text"
              onChange={(e) => setSalesTaxRate(e.target.value)}
            />
          </div>
          <div className="flex flex-col space-y-1">
            <label className="text-sm" htmlFor="maxItemPrice">
              Sort by
            </label>
            <select
              className="h-8 px-2 outline outline-1 outline-neutral-600 text-sm bg-transparent rounded"
              id="sortBySelect"
              onChange={(e) => {
                setSortBy(e.target.value);
              }}
            >
              <option className="" value="totalPriceAsc">
                Total Price Asc
              </option>
              <option className="w-full" value="totalPriceDesc">
                Total Price Desc
              </option>
              <option className="w-full" value="newest">
                Newest
              </option>
              <option className="w-full" value="oldest">
                Oldest
              </option>
            </select>
          </div>
          <button
            className="w-full h-8 px-4 text-sm text-white bg-blue-600 rounded-full"
            onClick={async () => {
              await fetchEbayListings();
            }}
          >
            Add Task
          </button>
        </div>
        <h4 className="text-sm mb-2">Showing {ebayListings.length} results</h4>
        <ul className="">
          {ebayListings.map((ebayListing) => {
            return (
              <li
                className="mb-4 border-b border-neutral-500"
                key={ebayListing.id}
              >
                <a className="text-blue-300 text-sm" href={ebayListing.urlLink}>
                  {ebayListing.title}
                </a>
                <div className="mb-4"></div>
                <div className="grid grid-cols-4 gap-2 mb-4">
                  <div className="">
                    <h6 className="font-bold text-sm">Site</h6>
                    <span className="text-sm">eBay</span>
                  </div>
                  <div className="">
                    <h6 className="font-bold text-sm">Condition</h6>
                    <span className="text-sm">{ebayListing.condition}</span>
                  </div>
                  <div className="">
                    <h6 className="font-bold text-sm">Recent</h6>
                    <span className="text-sm">
                      {ebayListing.isNewListing ? "Yes" : "No"}
                    </span>
                  </div>
                  <div className="">
                    <img
                      className="rounded"
                      src={ebayListing.imageUrl}
                      alt="Listing image"
                    />
                  </div>
                  <div className="">
                    <h6 className="font-bold text-sm">Subtotal</h6>
                    <span className="text-sm">${ebayListing.itemPriceUsd}</span>
                  </div>
                  <div className="">
                    <h6 className="font-bold text-sm">Shipping</h6>
                    <span className="text-sm">
                      {ebayListing.shippingPriceUsd > 0
                        ? ebayListing.shippingPriceUsd
                        : "Free"}
                    </span>
                  </div>
                  <div className="">
                    <h6 className="font-bold text-sm">Tax</h6>
                    <span className="text-sm">${ebayListing.salesTaxUsd}</span>
                  </div>
                  <div className="">
                    <h6 className="font-bold text-sm">Total</h6>
                    <span className="text-sm">
                      ${ebayListing.totalPriceUsd}
                    </span>
                  </div>
                </div>
                <div className="">
                  <div>
                    <span className="text-xs">Listed • </span>
                    <time className="text-xs">
                      {format(
                        parseISO(ebayListing.listedAt),
                        "MM/dd/yyyy hh:mm a",
                      )}
                    </time>
                  </div>
                </div>
              </li>
            );
          })}
        </ul>
      </div>
    </div>
  );
}
