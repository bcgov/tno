import env from 'env.json';
import { Sidebar } from 'react-pro-sidebar';
import styled from 'styled-components';

export const CustomSidebar = styled(Sidebar)`
  position: fixed;
  left: 0;
  .ps-sidebar-container {
    /* 500 is min width on chrome browser */
    @media (max-width: 500px) {
      max-width: 50px;
    }
    font-family: 'Source Sans Pro', sans-serif;
    background-color: ${(props) => props.theme.css.darkHeaderColor};
    min-height: 100vh;
    img {
      max-width: 100%;
      max-height: 100%;
    }
  }

  /* controls the mmia logo at top of sidebar */
  .title {
    background-color: ${(props) => {
      if (env.dev.includes(window.location.hostname))
        return props.theme.css.developmentBackgroundColor;
      else if (env.test.includes(window.location.hostname))
        return props.theme.css.testBackgroundColor;
      else return props.theme.css.productionBackgroundColor;
    }};
    padding: 0.65rem;
  }

  .ps-sidebar-root {
    @media (max-width: 500px) {
      max-width: 50px;
    }
    background-color: ${(props) => props.theme.css.darkHeaderColor};
    margin-right: 0px;
    border: none;
    min-height: -webkit-fill-available;

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
    color: ${(props) => props.theme.css.sideBarIconColor};
  }

  .ps-menu-button {
    /* need to override defaults for mobile devices with smaller screens */
    @media (max-width: 500px) {
      padding-left: 6px !important;
      padding-right: 6px !important;
    }
    /* again need to override defaults */
    &:hover {
      background-color: ${(props) => props.theme.css.selectedMenuItemColor} !important;
    }
  }
  .ps-menuitem-root {
    color: white;
    margin-bottom: 0.5rem;
  }

  .ps-menuitem-root:not(.selected) {
    background-color: ${(props) => props.theme.css.menuItemColor};
  }

  .label-container {
    .secondary-icon {
      &:hover {
        color: white;
      }
      color: ${(props) => props.theme.css.sideBarIconColor};
      margin-left: 70px;
      margin-top: auto;
      margin-bottom: auto;
      height: 16px;
      width: 16px;
    }
  }
`;
