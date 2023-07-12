import styled from 'styled-components';

export const Tabs = styled.div`
  display: flex;
  flex-flow: column;
  width: 100%;
  height: 100%;

  .tab {
    color: #494949;
    font-weight: bold;
    padding: 0.5em 1em 0.5em 1em;
    text-align: center;
    cursor: pointer;
    border-top-left-radius: 0.35em;
    border-top-right-radius: 0.35em;

    & .disabled {
      color: #999;
      cursor: not-allowed;
    }
  }
`;
