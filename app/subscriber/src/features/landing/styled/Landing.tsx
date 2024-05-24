import styled from 'styled-components';
import { Col } from 'tno-core';

export const Landing = styled(Col)`
  display: flex;
  .search-links {
    color: #3847aa;
    text-decoration: underline;
    &:hover {
      cursor: pointer;
    }
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
    .commentary-box {
      .headline {
        max-width: 30em;
      }
    }
    max-height: calc(100vh - 6.5em);
    @media (min-width: 1832px) {
      max-width: 44%;
    }
    margin-left: auto;
    flex-grow: 1;
    margin-right: auto;

    display: flex;
    flex-flow: column;

    input {
      min-height: 3em;
      border-radius: none;
      max-width: 60%;
    }
  }

  .title {
    background-color: white;
    padding: 0.5em;
    font-size: 1.35em;
    color: ${(props) => props.theme.css.btnRedColor};
    font-weight: bold;
    border-bottom: 1px solid #56537a;
  }
  /* The panel containing the media list */
  .main-panel {
    /* switch between max width and min width depending on screen size in order to maximize screen real estate */
    @media (max-width: 1702px) {
      min-width: 97%;
    }
    @media (max-width: 1000px) {
      margin-left: 0;
      margin-right: 0;
    }
    @media (min-width: 1000px) {
      width: 70%;
    }
    flex-grow: 1;
    margin: 0 0 0.75rem 0.75rem;

    .content {
      background-color: white;
      @media (max-width: 500px) {
        padding: 0.25em;
      }
      @media (min-width: 500px) {
        padding: 1em;
        padding-top: 0;
      }
      min-height: calc(75dvh);
      overflow-y: auto;
      overflow-x: clip;
    }
  }
`;
