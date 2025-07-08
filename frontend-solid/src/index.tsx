import 'uno.css';
import { render } from 'solid-js/web';
import { Route, Router } from "@solidjs/router";
import Home from './Routes/Home';
import Symbol from './Routes/Symbol';

const root = document.getElementById('root');

if (import.meta.env.DEV && !(root instanceof HTMLElement)) {
  throw new Error(
    'Root element not found. Did you forget to add it to your index.html? Or maybe the id attribute got misspelled?',
  );
}

render(
  () => (
    <Router>
      <Route path="/" component={Home} />
      <Route path="/symbol/:symbol" component={Symbol} />
    </Router>
  ),
  root!
);
