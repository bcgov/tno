import styled from 'styled-components';

export const MyColleagues = styled.div`
  .header-row {
    display: flex;
    align-items: center;
    font-size: 1rem;
    color: #333;
    margin-top: 0.5rem;
    margin-bottom: 0.25rem;
    margin-left: 0.75rem;
    border-bottom: 1px solid #ccc;

    .icon {
      font-size: 1.2rem;
      margin-right: 0.75rem;
    }

    .header-text {
      font-weight: bold;
      font-size: 1.1rem;
    }
  }

  .description {
    margin-left: 2.8rem;
  }
  .txt-filter {
    flex-direction: row;
    align-items: center;
    flex: 1;

    @media only screen and (max-width: 500px) {
      flex-direction: column;
      align-items: flex-start;
    }

    label {
      color: ${(props) => props.theme.css.fPrimaryColor};
      font-weight: 400;
    }

    > div {
      flex: 1;
      flex-wrap: nowrap;
      gap: 1rem;

      input {
        min-width: 200px;
      }
    }
  }

  .b7 {
    font-weight: 700;
  }

  .fs1 {
    font-size: 0.75rem;
  }

  .back {
    color: ${(props) => props.theme.css.linkGrayColor};
    font-weight: 400;
    cursor: pointer;
  }
`;
