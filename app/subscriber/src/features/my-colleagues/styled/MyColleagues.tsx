import styled from 'styled-components';

export const MyColleagues = styled.div`
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
