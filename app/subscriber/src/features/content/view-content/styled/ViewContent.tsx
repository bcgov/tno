import styled from 'styled-components';

export const ViewContent = styled.div`
  .summary {
    font-family: ${(props) => props.theme.css.fSecondary};
  }
  .info-bar {
    /* negative margins to bypass parent padding (PageSection) */
    margin-left: -1.25em;
    margin-right: -1.25em;
    padding-left: 1.25em;
    padding-right: 1.25em;
    font-size: 0.8rem;
    text-transform: uppercase;
    .byline {
      font-weight: 600;
    }
    .right-side {
      margin-left: auto;
    }
    .divider {
      margin: 0 0.5em;
    }
    .numeric-tone {
      margin-left: 0.5em;
      align-self: center;
    }
    .tone-group {
      margin-left: 0.5em;
    }
  }
  video {
    margin-top: 1em;
    margin-bottom: 1em;
    box-shadow: 0 0 10px 0 rgba(0, 0, 0, 0.2);
  }
  .headline {
    font-weight: 600;
    font-size: 1.75rem;
    margin-bottom: 0.5em;
    color: ${(props) => props.theme.css.hPrimaryColor};
  }
  .neg {
    color: ${(props) => props.theme.css.toneNegative};
  }
  .pos {
    color: ${(props) => props.theme.css.tonePositive};
  }
  .neut {
    color: ${(props) => props.theme.css.toneNeutral};
  }
  .transcribe-button {
    border: 2px solid rgb(0, 51, 102) !important;
    margin-left: auto;
    .text {
      color: black;
    }
    svg,
    .spinner {
      color: ${(props) => props.theme.css.highlightActive};
      align-self: center;
      margin-left: 0.5em;
    }
  }

  img {
    max-width: 100%;
    max-height: 100%;
  }
`;
