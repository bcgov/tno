import styled from 'styled-components';

export const ReportPreviewStatus = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0.5rem 1rem;
  border-left: 4px solid ${(props) => props.theme.css?.primaryColor};
  background-color: rgba(${(props) => props.theme.css?.primaryRgb}, 0.08);

  .status-row {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  .status-failed {
    color: ${(props) => props.theme.css?.dangerColor};
  }

  .status-error {
    overflow-wrap: anywhere;
  }
`;
