import styled from 'styled-components';

import { ILayoutProps } from '..';

export const Layout = styled.div<ILayoutProps>`
  .search-with-logout {
    grid-area: header;
  }

  .nav-bar {
    grid-area: nav-bar;
  }

  .contents-container {
    grid-area: content;
  }

  .grid-container {
    display: grid;
    transition: 300ms;
    background-color: ${(props) => props.theme.css.bkMain};
    grid-template-areas:
      'header header'
      'nav-bar content';
    grid-auto-columns: max-content auto;
    grid-auto-rows: auto;
  }
`;
