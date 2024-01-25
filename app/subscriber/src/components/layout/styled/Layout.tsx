import styled from 'styled-components';

import { ILayoutProps } from '..';

export const Layout = styled.div<ILayoutProps>`
  main {
    overflow: clip auto;
    height: calc(100dvh - 4.75rem);
  }
  .header {
    grid-area: header;
  }

  .nav-bar {
    grid-area: nav-bar;
  }

  .contents-container {
    grid-area: content;
  }

  .grid-container {
    height: 100dvh;
    display: grid;
    transition: 300ms;
    background-color: ${(props) => props.theme.css.bkMain};
    grid-template-areas:
      'header header'
      'nav-bar content';
    grid-auto-columns: max-content 8fr;
    grid-auto-rows: auto;
  }
`;
