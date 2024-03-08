import styled from 'styled-components';

export const Loading = styled.div`
  position: absolute;
  left: 0;
  top: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(${(props) => props.theme.css?.primaryRgb}, 0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 0;
`;

export default Loading;
