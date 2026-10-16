import styled from 'styled-components';

export const ContentReferenceList = styled.div`
  .filter {
    display: flex;
    flex-wrap: nowrap;
    align-items: flex-end;
    gap: 0.5rem;
    margin-bottom: 0.5rem;

    /* Each field sizes its own wrapper, the inputs fill it. */
    & > div {
      min-width: 0;
    }

    .frm-in {
      padding-bottom: 0;
      margin-bottom: 0;
      width: 100%;
    }

    .filter-date {
      flex: 0 0 10.5rem;
    }

    .filter-source {
      flex: 1 1 16rem;
      max-width: 24rem;
    }

    .filter-status {
      flex: 0 1 11rem;
    }

    .filter-uid {
      flex: 1 1 12rem;
      max-width: 20rem;
    }

    .filter-actions {
      display: flex;
      gap: 0.25rem;
      flex: 0 0 auto;
    }

    @media (max-width: 900px) {
      flex-wrap: wrap;
    }
  }

  .not-imported {
    color: #606060;
  }
`;
