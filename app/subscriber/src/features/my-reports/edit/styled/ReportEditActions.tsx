import styled from 'styled-components';

export const ReportEditActions = styled.div`
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  gap: 1rem;
  padding: 0.75rem;
  background: ${(props) => props.theme.css.bkTertiary};

  position: sticky;
  inset-block-end: 0;
`;
