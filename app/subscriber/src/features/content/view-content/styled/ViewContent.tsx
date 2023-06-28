import styled from 'styled-components';

export const ViewContent = styled.div`
  video {
    margin-top: 1em;
    margin-bottom: 1em;
    box-shadow: 0 0 10px 0 rgba(0, 0, 0, 0.2);
  }
  .headline-container {
    font-size: 1.25rem;
    font-weight: bold;
    justify-content: space-between;
    svg {
      align-self: center;
    }
  }
  .neg {
    color: #dc3545;
  }
  .pos {
    color: #20c997;
  }
  .neut {
    color: #ffc107;
  }
  .name-date,
  .source-name {
    font-size: 0.875rem;
    font-weight: 600;
  }

  .source-name {
    margin-top: 0;
  }
  .source-section {
    font-size: 0.875rem;
  }
  .transcribe-button {
    margin-left: auto;
    .text {
      color: black;
    }
    svg,
    .spinner {
      color: ${(props) => props.theme.css.itemActiveColor};
      align-self: center;
      margin-left: 0.5em;
    }
  }
`;
