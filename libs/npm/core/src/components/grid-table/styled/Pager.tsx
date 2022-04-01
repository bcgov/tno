import styled from 'styled-components';

export const Pager = styled.div`
  margin-top: 3%;
  margin-bottom: 3%;

  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  justify-content: center;

  & > button {
    display: flex;
    justify-content: center;
    align-items: center;
    background-color: transparent;
    color: black;
    border-color: black;
  }
`;
