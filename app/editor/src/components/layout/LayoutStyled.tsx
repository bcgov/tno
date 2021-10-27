import styled from 'styled-components';

export const Layout = styled.div`
  display: flex;
  flex-direction: column;
  align-items: stretch;
  height: 100vh;

  main {
    flex-grow: 1;
    overflow: auto;
    position: relative;
  }
`;

export default Layout;
