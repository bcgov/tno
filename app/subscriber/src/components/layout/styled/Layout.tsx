import styled from 'styled-components';

import { ILayoutProps } from '..';
import env from '../env.json';

export const Layout = styled.div<ILayoutProps>`
  &:not(.unauth) {
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
      overflow: clip;
      display: grid;
      transition: 300ms;
      background-color: ${(props) => {
        if (env.dev.includes(window.location.hostname))
          return props.theme.css.developmentBackgroundColor;
        else if (env.test.includes(window.location.hostname))
          return props.theme.css.testBackgroundColor;
        else return props.theme.css.bkMain;
      }};
      grid-template-areas:
        'header header'
        'nav-bar content';
      grid-auto-columns: max-content 8fr;
      grid-auto-rows: auto;
    }
  }
`;
