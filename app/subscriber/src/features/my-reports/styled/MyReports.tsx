import styled from 'styled-components';

export const MyReports = styled.div`
  .txt-filter {
    flex-direction: row;
    align-items: center;
    flex: 1;
    text-transform: uppercase;

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

  .report-schedule {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;

    > div:first-child {
      display: flex;
      flex-direction: row;
      justify-content: center;
      width: 50px;

      svg {
        color: ${(props) => props.theme.css.iconSecondaryColor};
        height: 40px;
        max-height: 40px;
        min-height: 40px;
        width: 40px;
        max-width: 40px;
        min-width: 40px;
      }
    }

    & label {
      text-transform: uppercase;
    }
  }

  .report-instance {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;

    > div:first-child {
      display: flex;
      flex-direction: row;
      justify-content: center;
      width: 50px;

      svg {
        color: ${(props) => props.theme.css.iconSecondaryColor};
        height: 30px;
        max-height: 30px;
        min-height: 30px;
        width: 30px;
        max-width: 30px;
        min-width: 30px;
      }
    }

    & label {
      text-transform: uppercase;
    }
  }

  .b7 {
    font-weight: 700;
  }

  .fs1 {
    font-size: 0.75rem;
  }
`;
