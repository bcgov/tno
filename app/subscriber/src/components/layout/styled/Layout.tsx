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

    & > .navbar:first-child {
      border-bottom: solid 1px #65799e;
    }
  }

  main {
    background-color: #f2f2f2;
    flex-grow: 1;
    padding: 10px;
    position: relative;
    overflow-y: auto;
    height: calc(100% - 100px);
    margin: 0px;
    padding: 0;
    display: flex;
    justify-content: center;
  }

  .navbar {
    & > div {
      margin-left: 5em;
    }
  }
`;
