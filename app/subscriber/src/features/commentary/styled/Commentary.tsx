import styled from 'styled-components';

export const Commentary = styled.div`
  @media (max-width: 1702px) {
    margin-top: 0.5em;
    min-width: 100%;
    margin-bottom: 0.5em;
  }

  .content-list {
    padding: 1em;
  }

  a.headline {
    @media (min-width: 1825px) {
      width: 20em;
    }
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .action-refresh svg {
    color: #04814d !important;
    transition: color 0.3s ease;
  }
  .action-refresh:hover svg {
    transform: rotate(-90deg);
  }
  &:active svg {
    color: #26e194 !important;
  }
`;
