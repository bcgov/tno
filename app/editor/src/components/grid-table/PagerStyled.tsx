import styled from 'styled-components';

export const PagerStyled = styled.div`
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  justify-content: flex-end;

  & > button {
    display: flex;
    justify-content: center;
    align-items: center;
  }
`;

export default PagerStyled;
