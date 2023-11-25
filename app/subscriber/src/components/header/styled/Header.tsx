import styled from 'styled-components';

export const Header = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1rem;
  padding: 0.75rem;
  margin: 0 0 0.75rem 0;
  border-bottom: solid 1px #56537a;
  min-height: calc(63px - 1.5rem); // Subtract padding.
  max-height: 63px;
`;
