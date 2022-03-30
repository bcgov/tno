import styled from 'styled-components';

export const Pager = styled.div`
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
