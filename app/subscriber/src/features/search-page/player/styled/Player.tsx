import styled from 'styled-components';
import { Row } from 'tno-core';

export const Player = styled(Row)`
  width: 100%;
  p {
    padding: 0;
    margin: 0;
  }

  video {
    margin-bottom: 0.5em;
  }
  .body {
    width: 100%;
    padding: 0.5em;
    background-color: ${(props) => props.theme.css.lightGray};
  }
  .player-headline {
    padding: 0;
    font-weight: bold;
    font-size: 1.15em;
  }
  .source {
    font-size: 0.75em;
  }
  .summary {
    margin-top: 0.5em;
  }
  .title {
    background-color: ${(props) => props.theme.css.darkHeaderColor};
    padding: 0.5em;
    font-size: 1.25em;
    color: white;
    width: 100%;
  }
`;
