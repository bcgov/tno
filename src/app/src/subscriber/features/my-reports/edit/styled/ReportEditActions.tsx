import styled from 'styled-components';

export const ReportEditActions = styled.div`
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  gap: 1rem;
  padding: 0.75rem 1rem 1.25rem 1rem;
  background: ${(props) => props.theme.css.bkTertiary};

  position: sticky;
  inset-block-end: 0;

  .icon-refresh svg {
    color: #04814d !important;
    transition: color 0.3s ease;
  }
  .icon-refresh:hover svg {
    transform: rotate(-90deg);
  }
  .icon-refresh:active svg {
    color: #26e194 !important;
  }
`;
