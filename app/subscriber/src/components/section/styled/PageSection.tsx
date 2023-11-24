import styled from 'styled-components';

export const PageSection = styled.div`
  display: flex;
  flex-direction: column;
  border-radius: 0.5rem;
  background: ${(props) => props.theme.css.bkSecondary};
  box-shadow: ${(props) => props.theme.css.boxShadow};
  align-self: stretch;
  margin: 1rem;
  min-width: fit-content;

  .page-section-title {
    margin: 1rem;
    color: ${(props) => props.theme.css.hPrimaryColor};
    font-size: 26px;
    font-weight: 600;
    line-height: 32.68px;
    border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};
    margin-bottom: 1rem;
  }

  > div:last-child {
    display: flex;
    flex-direction: column;
    padding: 1rem;
    min-width: fit-content;
    gap: 0.25rem;
  }
`;
