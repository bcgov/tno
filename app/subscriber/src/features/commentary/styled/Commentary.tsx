import styled from 'styled-components';

export const Commentary = styled.div`
  width: 99%;
  margin-bottom: 5%;
  div {
    margin-right: 0.5em;
  }

  .headline {
    color: #3847aa;
    cursor: pointer;
    /* underline on hover */
    &:hover {
      text-decoration: underline;
    }
  }

  .content {
    padding-top: 0.5em;
    background-color: #f9f9f9;
    min-height: 20em;
  }
`;
