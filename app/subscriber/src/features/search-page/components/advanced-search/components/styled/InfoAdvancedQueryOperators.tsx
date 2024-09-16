import styled from 'styled-components';

export const InfoAdvancedQueryOperators = styled.div`
  svg {
    cursor: pointer;
    color: ${(props) => props.theme.css.btnBkPrimary};
    margin-right: 0.25em;
  }

  .col-word-wrap {
    width: 34rem;
    word-wrap: break-word;
  }

  .col-basic {
    width: 3rem;
    word-wrap: break-word;
  }

  .col-advanced {
    width: 3rem;
    word-wrap: break-word;
  }

  .col-usage {
    width: 8rem;
    word-wrap: break-word;
  }

  .col-example {
    width: 20rem;
    word-wrap: break-word;
  }

  .table-header {
    background-color: lightblue;
  }

  .tooltip-fonts {
    font-size: 0.8rem;
  }
`;
