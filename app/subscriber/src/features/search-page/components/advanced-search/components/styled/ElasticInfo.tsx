import styled from 'styled-components';

export const ElasticInfo = styled.div`
  svg {
    cursor: pointer;
    color: ${(props) => props.theme.css.btnBkPrimary};
    margin-right: 0.25em;
  }

  code {
    font-size: 1em;
    background: ${(props) => props.theme.css.codeBlockColor};
    border: 1px solid ${(props) => props.theme.css.codeBlockColor};
    color: ${(props) => props.theme.css.fPrimaryColor};
    font-size: 15px;
    max-width: fit-content;
    overflow: auto;
    padding-left: 0.25em;
    padding-right: 0.25em;
    display: flex;
    margin-right: 0.25em;
    margin-bottom: 0.25em;
  }

  .row {
    max-height: 2.5em;
  }
`;
