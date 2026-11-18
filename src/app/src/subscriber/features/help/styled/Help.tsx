import styled from 'styled-components';

export const Help = styled.div`
  .help-content {
    align-items: stretch;
    margin: 2rem;
    padding: 0.75em;
    .description {
      font-weight: normal;
      margin-top: 1em;
    }
    h2 {
      border-bottom: 1px solid ${(props) => props.theme.css.iconGrayColor};
      color: ${(props) => props.theme.css.linkPrimaryColor};
      margin-bottom: 1rem;
    }
  }

  .list-icon {
    color: ${(props) => props.theme.css.iconGrayColor};
    margin-right: 0.25em;
  }

  .media-playback {
    video {
      height: 270px;
      width: 480px;
      margin-left: auto;
      margin-right: auto;
    }
    audio {
      margin-left: auto;
      margin-right: auto;
    }
    .copyright-text {
      margin-top: 0.5rem;
      font-size: 0.9rem;
      display: flex;
      margin-left: auto;
      margin-right: auto;
      color: ${(props) => props.theme.css.iconGrayColor};
      svg {
        margin-right: 0.25rem;
      }
    }
  }
`;
