import styled from 'styled-components';

export const Loader = styled.div`
  background: rgba(0, 0, 0, 0.25);
  position: absolute;
  z-index: 1000;
  top: 0;
  left: 0;
  height: 100%;
  width: 100%;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
`;
