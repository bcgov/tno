import styled from 'styled-components';

export const ReportEditPage = styled.div`
  form {
    display: flex;
    flex-direction: row;
    gap: 1rem;

    > div:first-child {
      flex: 1 1 30%;
      background: ${(props) => props.theme.css.bkSecondary};
    }

    > div:last-child {
      flex: 1 1 70%;
      background: ${(props) => props.theme.css.bkSecondary};
    }
  }
`;
