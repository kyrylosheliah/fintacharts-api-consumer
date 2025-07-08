import { createSignal, createEffect, on, onCleanup } from "solid-js";
import { IInstrument } from "../Models/Instrument";
import { IMarketBar } from "../Models/MarketBar";
import { IMarketQuoteMessage, IQuoteUpdate } from "../Models/MarketQuote";

const backendUrl = import.meta.env.VITE_BACKEND;

export function useSymbolStats(
  symbol: string,
) {
  const [error, setError] = createSignal<Error | null>(null);
  const [instrument, setInstrument] = createSignal<IInstrument | null>(null);
  const [history, setHistory] = createSignal<IMarketBar[]>([]);
  var socket: WebSocket = undefined!;
  const socketUrl = `${backendUrl}/api/v1/asset/ws/${symbol}`;
  const [ask, setAsk] = createSignal<IQuoteUpdate | undefined>();
  const [bid, setBid] = createSignal<IQuoteUpdate | undefined>();
  const [last, setLast] = createSignal<IQuoteUpdate | undefined>();

  const handleSocket = () => {
    socket = new WebSocket(socketUrl);
    socket.onmessage = (event) => {
      try {
        const data: IMarketQuoteMessage = JSON.parse(event.data);
        console.log(data);
        switch (data.type) {
          case "l1-snapshot":
            const quote = data.quote!;
            setAsk(quote.ask);
            setBid(quote.bid);
            setLast(quote.last);
            break;
          case "l1-update":
            if (data.ask !== undefined)
              setAsk(data.ask);
            else if (data.bid !== undefined)
              setBid(data.bid);
            else if (data.last !== undefined)
              setLast(data.last);
            break;
        }
      } catch (e) {
        console.error('Invalid WebSocket message:', e);
      }
    };
  };

  const fetchInstrument = async () => {
    const url = new URL(`${backendUrl}/api/v1/asset/instruments`);
    const allParams = {
      size: 1,
      symbol
    };

    Object.entries(allParams).forEach(([key, value]) => {
      url.searchParams.append(key, String(value));
    });

    await fetch(url.toString()).then(async (res) => {
      if (!res.ok) throw new Error(`Error: ${res.status}`);
      const json = await res.json();
      const instruments: IInstrument[] | undefined = json.data;
      if (instruments === undefined) throw Error("Bad response content");
      if (instruments.length < 1) throw Error("Bad response content");
      setInstrument(instruments[0]);
    }).catch((e) => {
      setError(e as Error);
    });
  };

  const refetchHistory = async () => {
    const url = `${backendUrl}/api/v1/asset/history/${symbol}`;
    await fetch(url).then(async (res) => {
      if (!res.ok) throw new Error(`Error: ${res.status}`);
      const json = await res.json();
      if (json.data === undefined) throw Error("Bad response content");
      setHistory(json.data);
    }).catch((e) => {
      setError(e as Error);
    });
  };

  createEffect(async () => {
    await fetchInstrument();
    handleSocket();
    return () => {
      socket.close();
    };
  });

  createEffect(
    on(instrument, () => {
      if (instrument() === null) return;
      refetchHistory();
    })
  );

  onCleanup(() => {
    if (socket !== undefined) socket.close();
  });

  return {
    error,
    instrument,
    history,
    ask,
    bid,
    last,
  };
}
