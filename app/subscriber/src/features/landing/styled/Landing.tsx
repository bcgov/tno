import styled from 'styled-components';
import { Col } from 'tno-core';

export const Landing = styled(Col)`
  overflow: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'BCSans', 'Noto Sans', Arial, 'sans serif';
  display: flex;

  .title {
    font-family: 'Source Sans Pro', sans-serif;
  }

  /* header for viewing a piece of content on the landing page */
  .view {
    svg {
      cursor: pointer;
      color: #a5a4bf;
    }
  }

  /* The panel containing Commentary and front pages */
  .right-panel {
    min-width: 39%;
    margin-left: 0.5%;
    flex-grow: 1;
    .title {
      background-color: ${(props) => props.theme.css.darkHeaderColor};
      padding: 0.5em;
      font-size: 1.25em;
      color: white;
    }

    .front-pages {
      width: 99%;
      margin-bottom: 2%;
      .content {
        background-color: #f9f9f9;
        min-height: 10em;
      }
    }
    display: flex;

    input {
      min-height: 3em;
      border-radius: none;
      max-width: 60%;
    }
  }

  /* container containing both panels */
  .contents-container {
    overflow-y: auto;
    max-height: calc(100vh - 6.5em);
  }

  /* The panel containing the media list */
  .main-panel {
    /* switch between max width and min width depending on screen size in order to maximize screen realestate */
    @media (max-width: 1000px) {
      min-width: 59%;
    }
    @media (min-width: 1000px) {
      max-width: 59%;
    }
    flex-grow: 1;
    margin-left: 0.5%;

    .title {
      background-color: ${(props) => props.theme.css.darkHeaderColor};
      padding: 0.5em;
      font-size: 1.75em;
      color: white;
    }
    .content {
      background-color: ${(props) => props.theme.css.lightGray};
      @media (max-width: 500px) {
        padding: 0.25em;
      }
      @media (min-width: 500px) {
        padding: 1em;
      }
      min-height: 45em;
    }

    /* TODO: move these to the button component as styling configuration */
    button {
      margin-bottom: 10%;
      min-width: 5rem;
      border: none;
      display: flex;
      justify-content: center;
      &.active {
        background-color: ${(props) => props.theme.css.defaultRed} !important;
        color: white;
      }
      &.inactive {
        background-color: ${(props) => props.theme.css.lightInactiveButton} !important;
        color: #7a7978;
      }
    }
  }
`;
