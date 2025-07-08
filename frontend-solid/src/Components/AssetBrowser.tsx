import { Component, Show, For } from "solid-js";
import { useAssetSearch } from "../Hooks/useAssetSearch";

export const AssetBrowser: Component = () => {
  const {
    data,
    loading,
    error,
    page,
    setPage,
    totalPages,
    size,
    setSize,
  } = useAssetSearch();

  return (
    <div>
      <h2 class="text-xl font-bold mb-2">Assets</h2>

      <Show when={loading()}>
        <p>Loading...</p>
      </Show>

      <Show when={error()}>
        <p class="text-red-500">{error()!.message}</p>
      </Show>

      <Show when={data()}>
        <For each={data().data}>
          {(item) => (
            <div>
              <a
                href={`/symbol/${item.symbol}`}
                children={`${item.symbol}: ${item.description}`}
              />
            </div>
          )}
        </For>

        <div class="mt-4 flex gap-2 items-center">
          <button
            onClick={() => setPage((p) => Math.max(p - 1, 1))}
            disabled={page() === 1}
          >
            Prev
          </button>
          <span>
            Page {page()} of {data().paging.pages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(p + 1, totalPages()))}
            disabled={page() === totalPages()}
          >
            Next
          </button>
        </div>
      </Show>
    </div>
  );
};
