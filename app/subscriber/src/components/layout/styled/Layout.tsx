import styled from 'styled-components';

import { ILayoutProps } from '..';

export const Layout = styled.div<ILayoutProps>`
  /* Control styling and width behaviour of sidebar when it is resized */

  .collapse {
    color: white;
    &:hover {
      cursor: pointer;
    }
    ${(props) => (props.collapsed ? 'margin-left: 60px;' : 'margin-left: 230px;')}}};
  }

  /* 2 column base grid, sidebar left column, main content right column */
  .grid-container {
    background-color: ${(props) => props.theme.css.beigeBackgroundColor};
    display: grid;
    ${(props) =>
      props.collapsed
        ? 'grid-template-columns: 80px auto;'
        : 'grid-template-columns: 250px auto;'}}};
  }

  .main-contents {
    background-color: ${(props) => props.theme.css.beigeBackgroundColor};
  }
`;
