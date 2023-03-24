import { Sidebar } from 'react-pro-sidebar';
import styled from 'styled-components';

export const CustomSidebar = styled(Sidebar)`
  /* Side bar styling */
  height: 100vh;
  .ps-sidebar-container {
    background-color: ${(props) => props.theme.css.darkHeaderColor};
    img {
      max-width: 100%;
      max-height: 100%;
    }
  }

  /* controls the mmia logo at top of sidebar */
  .title {
    background-color: #302e2e;
    padding: 0.65rem;
  }

  .ps-sidebar-root {
    background-color: ${(props) => props.theme.css.darkHeaderColor};
    margin-right: 0px;

    min-height: 100vh;
    display: flex;
  }

  .ps-menu-root {
    ul {
      background-color: ${(props) => props.theme.css.darkHeaderColor};
    }
  }

  .selected {
    background-color: ${(props) => props.theme.css.selectedMenuItemColor};
    border-left: 3px solid ${(props) => props.theme.css.itemActiveColor};
  }

  .ps-menu-icon {
    color: #a5a4bf;
  }

  .menu-anchor {
    &:hover {
      background-color: red;
    }
  }
  .ps-menuitem-root {
    color: white;
    margin-bottom: 0.5rem;
  }

  .ps-menuitem-root:not(.selected) {
    background-color: ${(props) => props.theme.css.menuItemColor};
  }
`;
