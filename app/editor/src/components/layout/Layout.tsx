import './Layout.css';

import logo from './logo.svg';

export const Layout: React.FC = (props) => {
  return (
    <div className="App">
      <header>header</header>
      <main>
        <img src={logo} className="App-logo" alt="logo" />
        {props.children}
      </main>
      <footer>footer</footer>
    </div>
  );
};
