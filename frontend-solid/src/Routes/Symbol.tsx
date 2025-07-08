import { useParams } from '@solidjs/router';
import { Show, type Component } from 'solid-js';
import { SymbolStats } from '../Components/SymbolStats';

const Symbol: Component = () => {
  const params = useParams();
  return (
    <div class="p-20">
      <Show when={params.symbol} fallback={<p>No asset specified</p>}>
        <SymbolStats symbol={params.symbol} />
      </Show>
    </div>
  );
};

export default Symbol;
