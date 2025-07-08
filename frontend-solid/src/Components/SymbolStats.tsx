import { Component, For, Show } from "solid-js";
import { useSymbolStats } from "../Hooks/useSymbolStats";

export const SymbolStats: Component<{ symbol: string; }> = (params) => {
  const { error, instrument, history, ask, bid, last } = useSymbolStats(
    params.symbol
  );

  return (
    <div>
      <Show when={error()}>
        <p class="text-red-500">{error()!.message}</p>
      </Show>

      <Show when={instrument()}>
        <h2 class="text-xl font-bold mb-2">{`Asset page (${instrument().symbol})`}</h2>

        <p>{`${instrument().symbol}: ${instrument().description}`}</p>

        
        <p>{"Ask: "}{JSON.stringify(ask())}</p>
        <p>{"Bid: "}{JSON.stringify(bid())}</p>
        <p>{"Last: "}{JSON.stringify(last())}</p>

        <div>
          <p>History:</p>
          <For each={history()}>
            {(bar) => <div>{JSON.stringify(bar)}</div>}
          </For>
        </div>
      </Show>
    </div>
  );
};
