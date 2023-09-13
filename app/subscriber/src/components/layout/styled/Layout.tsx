import styled from 'styled-components';

import { ILayoutProps } from '..';

export const Layout = styled.div<ILayoutProps>`
  /* Control styling and width behaviour of sidebar when it is resized */
  .collapse {
    color: white;
    &:hover {
      cursor: pointer;
    }
    ${(props) => (props.collapsed ? 'margin-left: 60px;' : 'margin-left: 230px;')}
  }

  /* 2 column base grid, sidebar left column, main content right column */
  .grid-container {
    background-color: ${(props) => props.theme.css.beigeBackgroundColor};
    /* overflow: hidden; */
    display: grid;
    /* media query for screens bigger than 500 */
    @media (min-width: 500px) {
      ${(props) =>
        props.collapsed
          ? 'grid-template-columns: 80px auto;'
          : 'grid-template-columns: 250px auto;'}
    }
    /* media query for screens less than 500 */
    @media (max-width: 500px) {
      grid-template-columns: 50px auto;
    }
  }

  .main-contents {
    @media (max-width: 500px) {
      z-index: 1;
    }
    background-color: ${(props) => props.theme.css.beigeBackgroundColor};
  }
`;
