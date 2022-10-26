import styled from 'styled-components';

export const IngestDetails = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1em;

  & > div {
    flex-grow: 1;
    flex-basis: max-content;
  }

  hr {
    width: 100%;
  }

  .locations {
    background: ${(props) => props.theme.css.tableColor};

    div[direction='row'] {
      > div:first-child {
        padding-left: 1em;
      }
    }

    div[direction='row']:nth-child(2n + 2) {
      background: ${(props) => props.theme.css.light};
    }
  }
`;
