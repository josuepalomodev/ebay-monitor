import { useEffect, useState } from "react";
import { EbayListing } from "./types.ts";
import format from "date-fns/format";
import { parseISO } from "date-fns";

export default function App() {
  const [ebayListings, setEbayListings] = useState<EbayListing[]>([]);

  useEffect(() => {
    async function fetchEbayListings(): Promise<void> {
      const serverApiEndpointTest =
        "https://localhost:44313/EbayListings?searchQuery=6700xt";
      try {
        const response = await fetch(serverApiEndpointTest);
        if (response.ok) {
          setEbayListings((await response.json()) as EbayListing[]);
        }
      } catch (e) {
        console.log(e);
      }
    }

    fetchEbayListings();
  }, []);

  return (
    <div className="min-h-screen max-w-screen text-base text-neutral-300 bg-neutral-900">
      <div className="px-4 my-4">
        <h4 className="text-xl">Feed</h4>
      </div>
      <div className="px-4 mb-2">
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
