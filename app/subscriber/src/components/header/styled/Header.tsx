import styled from 'styled-components';

export const Header = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1rem;
  padding: 0.75rem;

  @media (max-width: 700px) {
    .mm-logo {
      display: none;
    }
  }

  @media (min-width: 700px) {
    .mm-logo-no-text {
      display: none;
    }
  }

  .logo-container {
    @media (max-width: 700px) {
      width: fit-content;
    }
    @media (min-width: 700px) {
      width: 15em;
    }
    display: flex;
  }
`;
