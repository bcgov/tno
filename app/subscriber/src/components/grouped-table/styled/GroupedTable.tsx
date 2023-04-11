import styled from 'styled-components';

export const GroupedTable = styled.table`
  width: 100%;
  border-spacing: 0;
  box-shadow: 0 0 10px 0 rgba(0, 0, 0, 0.1);
  .main-header {
    color: #7c7e8a;
    background-color: #f5f6fa;
    font-size: 0.85rem;
    margin: 0;
    padding: 0.5rem;
    text-align: left;
  }
  .group-title {
    color: #4d4f5c;
    text-align: left;
    /* for screens bigger than 500px */
    @media (min-width: 500px) {
      font-size: 1.25rem;
    }
    /* for screens smaller than 500px */
    @media (max-width: 500px) {
      font-size: 1rem;
    }
    padding: 0.5rem;
  }
  .content-rows {
    background-color: white;
    td {
      padding: 0.25rem 0.5rem 0.25rem;
    }
    &:hover {
      background-color: #f5f6fa;
      cursor: pointer;
    }
  }
`;
