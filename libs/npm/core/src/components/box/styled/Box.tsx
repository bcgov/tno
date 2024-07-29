import styled from 'styled-components';

export const Box = styled.div`
  position: relative;
  width: 100%;
  height: 100%;

  .overlay {
    width: 100%;
    height: 100%;
    position: absolute;
    top: 0;
    left: 0;
    z-index: 10;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: rgba(0, 0, 0, 0.25);
    border-radius: 0.25rem;
  }
`;
