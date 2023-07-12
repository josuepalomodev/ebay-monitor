export interface EbayListing {
  id: string;
  urlLink: string;
  title: string;
  isNewListing: boolean;
  imageUrl: string;
  condition: string;
  itemPriceUsd: number;
  shippingPriceUsd: number;
  salesTaxUsd: number;
  totalPriceUsd: number;
  listedAt: string;
}
