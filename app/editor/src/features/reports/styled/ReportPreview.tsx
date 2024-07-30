import styled from 'styled-components';

export const ReportPreview = styled.div`
  max-width: 100%;
  min-width: 100%;

  .preview-report {
    align-items: stretch;
    margin: 2rem;

    > .preview-subject {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      flex: 1;
      background-color: white;
      padding: 1rem;
      border-top-right-radius: 0.5rem;
      border-top-left-radius: 0.5rem;
      border-bottom: solid 1px black;
      box-shadow: 10px 5px 10px grey;
    }

    > .preview-body {
      flex: 1;
      background-color: white;
      padding: 1rem;
      border-bottom-right-radius: 0.5rem;
      border-bottom-left-radius: 0.5rem;
      box-shadow: 10px 5px 10px grey;

      img {
        max-width: 80%;
      }
    }

    .email-body {
      display: none;
    }
  }
`;
