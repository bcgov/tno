import styled from 'styled-components';

export const Header = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1rem;
  padding: 0.75rem;
  margin: 0 0 0.75rem 0;
  border-bottom: solid 1px ${(props) => props.theme.css.primaryLineColor};
  min-height: calc(63px - 1.5rem); // Subtract padding.
  max-height: 63px;

  .logo-container {
    width: 15em;
    .mm-logo {
      width: 100%;
    }
  }
`;
