import { createSignal, createEffect, on } from "solid-js";
import { IInstrumentResponse } from "../Models/Instrument";

const backendUrl = import.meta.env.VITE_BACKEND;

export function useAssetSearch(
  //initialParams: Record<string, string | number> = {}
) {
  //const [providers, setProviders] = createSignal<string[]>([]);

  const [data, setData] = createSignal<IInstrumentResponse | null>(null);
  const [loading, setLoading] = createSignal(false);
  const [error, setError] = createSignal<Error | null>(null);

  const [page, setPage] = createSignal(1);
  const [size, setSize] = createSignal(10);
  //const [provider, setProvider] = createSignal<string>();

  const totalPages = () => data() ? data().paging.pages : 0;

  const init = async () => {

  };

  createEffect(() => {
    init();
  });

  const refetch = async () => {
    setLoading(true);
    setError(null);

    const url = new URL(`${backendUrl}/api/v1/asset/instruments`);
    const allParams = {
      page: page(),
      size: size()
    };

    Object.entries(allParams).forEach(([key, value]) => {
      url.searchParams.append(key, String(value));
    });

    try {
      const res = await fetch(url.toString());
      if (!res.ok) throw new Error(`Error: ${res.status}`);
      const json = await res.json();
      setData(json);
    } catch (e) {
      setError(e as Error);
    } finally {
      setLoading(false);
    }
  };

  createEffect(
    on([page, size], () => {
      refetch();
    })
  );

  return {
    data,
    loading,
    error,
    page,
    setPage,
    size,
    setSize,
    totalPages,
  };
}
