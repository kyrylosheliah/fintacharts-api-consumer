import { Component, For, Show } from "solid-js";
import { useSymbolStats } from "../Hooks/useSymbolStats";

export const SymbolStats: Component = (params: { symbol: string }) => {
  const { error, instrument, history, ask, bid, last } = useSymbolStats(
    params.symbol
  );

  return (
    <div>
      <h2 class="text-xl font-bold mb-2">Assets</h2>

      <Show when={error()}>
        <p class="text-red-500">{error()!.message}</p>
      </Show>

      <Show when={instrument()}>
        <p>{`${instrument().symbol}: ${instrument().description}`}</p>

        <div>
          <For each={history()}>
            {(bar) => <div>{JSON.stringify(bar)}</div>}
          </For>
        </div>

        <p>{JSON.stringify(ask())}</p>
        <p>{JSON.stringify(bid())}</p>
        <p>{JSON.stringify(last())}</p>
      </Show>
    </div>
  );
};
