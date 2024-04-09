import styled from 'styled-components';

export const ReportEditActions = styled.div`
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  gap: 1rem;
  padding: 0.25rem 1rem;
  background: ${(props) => props.theme.css.bkTertiary};

  position: sticky;
  bottom: 0;
  left: 0;
  right: 0;
`;
