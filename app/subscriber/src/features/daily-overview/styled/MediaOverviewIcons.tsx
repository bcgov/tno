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

  .scroll-icon {
    height: 1.5em;
    width: 1.5em;
    color: #23b6d4;
    padding-right: 1em;
  }

  .video-icon {
    height: 1.5em;
    width: 1.5em;
    color: #22c997;
    padding-right: 1em;
  }
`;
