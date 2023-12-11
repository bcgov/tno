import styled from 'styled-components';

export const AddToMenu = styled.div`
  .report {
    svg {
      margin-right: 0.5em;
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }
    font-weight: bold;
  }
  .list {
    margin-left: 1.5em;
    .row-title {
      font-weight: bold;
    }
    .report-item {
      cursor: pointer;
      &:hover {
        background-color: ${(props) => props.theme.css.codeBlockColor};
      }
      min-width: 15em;
    }
    .expand-sections {
      margin-left: auto;
      align-self: center;
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }
  }
`;
