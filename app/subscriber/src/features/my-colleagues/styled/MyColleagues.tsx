import styled from 'styled-components';

export const MyColleagues = styled.div`
  .colleague-block {
    display: flex;
    gap: 0.5rem;
    padding: 0.8em;

    .colleague-title {
      font-weight: bold;
      font-size: 1.1rem;
      border-bottom: 1px solid #ccc;

      .icon {
        font-size: 1.3rem;
        margin-right: 0.5rem;
        vertical-align: middle;
      }
    }
    .colleague-describe {
      font-size: 1rem;
      margin-bottom: 0.8rem;
    }
    .request-button {
      margin-left: 1em;
    }

    .colleague-body {
      margin-top: 0.8rem;
      margin-left: 2.8rem;
    }

    .colleague-table .grid-header {
      border-top: none;
      border-bottom: 1px solid #ccc;
    }
  }
`;
