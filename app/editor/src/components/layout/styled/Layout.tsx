import styled from 'styled-components';

export const Layout = styled.div`
  display: flex;
  flex-direction: column;
  align-items: stretch;
  height: 100vh;

  .main-window {
    display: flex;
    flex-direction: column;
    align-items: stretch;
    flex-grow: 1;
    overflow: hidden;
  }

  main {
    flex-grow: 1;
    padding: 10px;
    position: relative;
    overflow-y: auto;
    height: calc(100% - 100px);
  }
`;
