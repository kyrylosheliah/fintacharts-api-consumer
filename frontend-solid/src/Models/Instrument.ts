export interface IInstrumentResponse
{
  paging: IInstrumentPagination;
  data: IInstrument[];
}

export interface IInstrument
{
  id: string;
  symbol: string;
  kind: string;
  description: string;
  tickSize: number;
  currency: string;
  baseCurrency: string;
  mappings: {[key: string]: IInstrumentMapping; };
  profile: IInstrumentProfile;
}

export interface IInstrumentMapping
{
  symbol: string;
  exchange: string;
  defaultOrderSize: number;
  maxOrderSize: number;
  tradingHours: IMappingTradingHours;
}

export interface IMappingTradingHours
{
  regularStart: string;
  regularEnd: string;
  electronicStart: string;
  electronicEnd: string;
}

export interface IInstrumentProfile
{
  name: string;
  gics: object;
}

export interface IInstrumentPagination
{
  page: number;
  pages: number;
  items: number;
}
