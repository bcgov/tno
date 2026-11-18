import styled from 'styled-components';

export const ReportStatus = styled.div`
  display: flex;
  flex: column;
  align-items: center;
  padding: 0 0.5rem;
  border-radius: 0.5rem;

  &.Failed {
    background: ${(props) => props.theme.css.btnBkError};
    color: ${(props) => props.theme.css.btnErrorColor};
  }

  &.Cancelled {
    background: ${(props) => props.theme.css.btnBkWarn};
    color: ${(props) => props.theme.css.btnWarnColor};
  }

  &.Completed {
    background: ${(props) => props.theme.css.btnBkSuccess};
    color: ${(props) => props.theme.css.btnSuccessColor};
  }
`;
