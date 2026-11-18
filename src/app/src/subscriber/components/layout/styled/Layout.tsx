import styled from 'styled-components';

import { ILayoutProps } from '..';

export const Layout = styled.div<ILayoutProps>`
  &.popout {
    .headline {
      margin-bottom: 0;
      padding-left: 0.25em;
    }
    .info-bar {
      margin-left: 0;
      margin-right: 0;
      padding-left: 0.5em;
      padding-right: 0.5em;
    }
  }
  &:not(.unauth):not(.popout) {
    main {
      overflow: clip auto;
      height: calc(100dvh - 4.75rem);
    }
    .header {
      grid-area: header;
    }

    .search-bar {
      grid-area: search-bar;
    }

    .nav-bar {
      grid-area: nav-bar;
      max-height: calc(100dvh - 11.5em);
      overflow: auto;
    }

    .contents-container {
      grid-area: content;
    }

    @media (max-width: 900px) {
      .grid-container {
        grid-template-areas:
          'header header'
          'search-bar search-bar'
          'nav-bar content';
      }
    }
    @media (min-width: 900px) {
      .search-bar {
        display: none;
      }
      .grid-container {
        grid-template-areas:
          'header header'
          'nav-bar content';
      }
    }
    .grid-container {
      height: 100dvh;
      overflow: clip;
      display: grid;
      transition: 300ms;
      background-color: ${(props) => props.theme.css.bkMain};
      grid-auto-columns: max-content 8fr;
      grid-auto-rows: auto;
    }
  }
`;
