import styled from 'styled-components';

export const ListView = styled.div`
  display: flex;
  flex-direction: column;

  input[type='text'],
  select {
    min-width: 15em;
  }

  .filter {
    display: flex;
    flex-direction: row;

    h2 {
      border: 0;
      padding: 0;
      margin-top: 0;
      font-weight: 400;
      font-size: 1em;
    }

    & > div:first-child {
      max-width: 20em;
    }

    & > div:nth-child(2) {
      flex-grow: 2;

      & > div:nth-child(2) {
        display: flex;
        flex-direction: row;
      }
    }
  }
`;
