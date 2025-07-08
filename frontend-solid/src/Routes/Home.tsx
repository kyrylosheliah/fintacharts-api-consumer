import type { Component } from 'solid-js';
import { AssetBrowser } from '../Components/AssetBrowser';

const Home: Component = () => {
  return (
    <div class="p-20">
      <AssetBrowser/>
    </div>
  );
};

export default Home;
