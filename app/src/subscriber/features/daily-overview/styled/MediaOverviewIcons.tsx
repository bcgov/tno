import styled from 'styled-components';

export const MediaOverviewIcons = styled.div`
  @media (max-width: 1702px) {
    margin-top: 0.5em;
    min-width: 100%;
    margin-bottom: 0.5em;
  }

  @media (min-width: 1702px) {
    margin-bottom: 5%;
  }

  .content {
    padding: 1em;
    background-color: white;
    min-height: 10em;
    max-height: 20em;
    overflow-y: auto;
    .content-row {
      svg {
        margin-right: 0.35em;
      }
    }
  }

  .content > * {
    display: flex;
    margin-bottom: 0.5em;
  }

  span {
    align-items: center;
  }

  .transcript-icon {
    height: 1.25em !important;
    width: 1.25em !important;
    color: ${(props) => props.theme.css.overviewTranscript};
    padding-right: 1em;
  }

  .video-icon {
    height: 1.25em !important;
    width: 1.25em !important;
    color: ${(props) => props.theme.css.overviewVideo};
    padding-right: 1em;
  }
`;
