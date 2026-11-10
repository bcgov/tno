import styled from 'styled-components';
export const TopStories = styled.div`
  .table {
    width: 100%;
    .group {
      background-color: #f9f9f9 !important;
    }
    .header {
      background-color: #f5f6fa;
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
    }
  }
  .headline {
    &:hover {
      text-decoration: underline;
    }
  }
`;
