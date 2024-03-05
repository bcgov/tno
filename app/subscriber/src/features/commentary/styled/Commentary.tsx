import styled from 'styled-components';

export const Commentary = styled.div`
  @media (max-width: 1702px) {
    margin-top: 0.5em;
    min-width: 100%;
    margin-bottom: 0.5em;
  }

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
    background-color: white;
    min-height: 10em;
    max-height: 20em;
    overflow-y: auto;
    .content-row {
      svg {
        margin-right: 0.35em;
      }
    }
  }
`;
