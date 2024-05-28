import styled from 'styled-components';

export const ViewContent = styled.div`
  .summary {
    font-family: ${(props) => props.theme.css.fPrimary};
  }
  .info-bar {
    /* negative margins to bypass parent padding (PageSection) */
    margin-left: -1.25em;
    margin-right: -1.25em;
    padding-left: 1.25em;
    padding-right: 1.25em;
    font-size: 0.9rem;
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
    margin-left: auto;
  }

  img {
    max-width: 100%;
    max-height: 100%;
  }

  ul.quotes-container {
    list-style-type: none;
    margin: 0rem 2rem;
    padding: 0;
    li {
      margin-bottom: 1rem;
    }
    label.quote-byline {
      font-weight: bold;
      font-style: italic;
      padding-left: 1rem;
    }
  }

  .transcript-status {
    background: ${(props) => props.theme.css.stickyNoteColor};
    padding: 1rem;
    border-radius: 0.5rem;
  }
`;
