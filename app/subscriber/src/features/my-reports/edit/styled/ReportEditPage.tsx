import styled from 'styled-components';

export const ReportEditPage = styled.div`
  form {
    display: flex;
    flex-direction: row;
    gap: 1rem;

    > div {
      flex: 1 1 100%;
      background: ${(props) => props.theme.css.bkSecondary};
    }
  }
`;
