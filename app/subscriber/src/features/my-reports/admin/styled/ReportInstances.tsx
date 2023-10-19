import styled from 'styled-components';

export const ReportInstances = styled.div`
  background-color: white;
  .header-row {
    background-color: ${(props) => props.theme.css.darkHeaderColor};
    padding: 0.5em;
    font-size: 1.75em;
    color: white;
    font-family: 'Source Sans Pro', sans-serif;
    .title {
      margin-left: auto;
    }
    .back-arrow {
      margin-top: 0.15em;
      &:hover {
        &:hover {
          cursor: pointer;
          transform: scale(1.1);
          color: ${(props) => props.theme.css.sideBarIconHoverColor};
        }
      }
      color: #a5a4bf;
    }
  }

  .table {
    width: 100%;
    .group {
      background-color: #f9f9f9 !important;
      font-family: ${(props) => props.theme.css?.bcSans};
    }
    .header {
      background-color: #f5f6fa;
      font-family: 'Roboto', sans-serif;
      font-size: 0.8em;
      /* box shadow only on bottom */
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
      border: none;
      color: #7c7e8a;

      .column {
        background-color: #f5f6fa;
      }
    }
    .rows {
      cursor: pointer;
      svg {
        height: 1.35em;
        width: 1.35em;
        color: #6750a4;
        &:hover {
          color: ${(props) => props.theme.css.sideBarIconHoverColor};
          transform: scale(1.1);
        }
      }
    }
  }
  .headline {
    /* link color */
    color: #3847aa;
  }

  .back-arrow {
    margin-top: 0.15em;
    &:hover {
      &:hover {
        cursor: pointer;
        transform: scale(1.1);
        color: ${(props) => props.theme.css.sideBarIconHoverColor};
      }
    }
    color: #a5a4bf;
  }
`;
