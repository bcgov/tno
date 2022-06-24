import styled from 'styled-components';

export const ContentForm = styled.div`
  #txa-summary {
    height: 6.25em;
  }

  .condensed {
    border: solid 1px grey;
    border-radius: 0.25em;
    .popout {
      cursor: pointer;
    }
    .popout:hover {
      color: #3498db;
    }
  }

  .checkbox-column {
    margin-left: 2em;

    & > div:first-child {
      max-height: 12em;
    }
  }

  .content-properties {
    padding-top: 1em;
  }

  .licenses {
    padding-left: 2em;
  }

  .hidden {
    display: none;
  }
`;
