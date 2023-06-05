import styled from 'styled-components';

export const Commentary = styled.div`
  max-width: 100%;
  min-width: 35em;
  margin-bottom: 5%;

  .headline {
    color: #3847aa;
    cursor: pointer;
    text-overflow: ellipsis;
    max-width: 32em;
    white-space: nowrap;
    overflow: hidden;
    /* underline on hover */
    &:hover {
      text-decoration: underline;
    }
  }

  .content {
    padding-top: 0.5em;
    background-color: ${(props) => props.theme.css.lightGray};
    min-height: 20em;
  }
`;
