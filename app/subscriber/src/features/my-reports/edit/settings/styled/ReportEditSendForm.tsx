import styled from 'styled-components';

export const ReportEditSendForm = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1rem 2rem;

  > h2 {
    border-bottom: solid 1px;
  }

  > div {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .frm-in > label {
    text-transform: uppercase;
    font-size: 0.85rem;
  }

  .schedules {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    justify-content: space-evenly;

    .schedule {
      padding: 0.5rem 2rem;
      border: solid 1px ${(props) => props.theme.css.bkQuaternary};
    }
  }

  .schedule-actions {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;
    justify-content: space-around;
  }

  .report-kind {
    .info {
      padding: 0 1rem;
      background: ${(props) => props.theme.css.bkInfo};
      border-top-left-radius: 1rem;
      border-top-right-radius: 1rem;
    }
  }
`;
