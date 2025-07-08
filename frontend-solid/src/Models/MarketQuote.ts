export interface IQuote {
  timestamp: string;
  price: number;
  volume: number;
}

export interface IMarketQuote
{
  ask?: IQuote;
  bid?: IQuote;
  last?: IQuote;
}

export type IQuoteUpdate = IQuote & {
  change?: number;
  changePct?: number;
}

export interface IMarketQuoteMessage {
  type: string;
  instrumentId?: string;
  provider?: string;
  quote?: IMarketQuote;
  ask?: IQuoteUpdate;
  bid?: IQuoteUpdate;
  last?: IQuoteUpdate;
}