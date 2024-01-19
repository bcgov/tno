import styled from 'styled-components';

export const Header = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1rem;
  padding: 0.75rem;
  border-bottom: solid 1px ${(props) => props.theme.css.primaryLineColor};

  .logo-container {
    width: 15em;
    display: flex;
  }
`;
