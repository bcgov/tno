import styled from 'styled-components';

export const ViewContent = styled.div`
  video {
    margin-top: 1em;
    margin-bottom: 1em;
    box-shadow: 0 0 10px 0 rgba(0, 0, 0, 0.2);
  }
  .headline-container {
    font-size: 1.25rem;
    font-weight: bold;
    justify-content: space-between;
    svg {
      align-self: center;
    }
  }
  .neg {
    color: #dc3545;
  }
  .pos {
    color: #20c997;
  }
  .neut {
    color: #ffc107;
  }
  .name-date {
    font-size: 0.875rem;
    font-weight: 600;
  }
  .source-section {
    font-size: 0.875rem;
  }
`;
