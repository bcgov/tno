import { PageSection } from 'components/section';
import styled from 'styled-components';

export const ConfigureFolder = styled(PageSection)`
  .react-datepicker-ignore-onclickoutside {
    border-radius: 0.5rem;
  }
  .back-to-folders {
    cursor: pointer;
    color: ${(props) => props.theme.css.btnBkPrimary};
    font-size: 1rem;
    &:hover {
      text-decoration: underline;
    }
  }
  .name {
    margin-left: auto;
  }
  button:not(.react-datepicker__close-icon) {
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    border: none;
    border-radius: 1rem;
    max-width: fit-content;
  }
  .react-datepicker__close-icon::after {
    background-color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .main-container {
    margin-left: 1rem;
    margin-right: 1rem;
    .frm-in {
      width: 95%;
    }
    .add-filter {
      border: 1px solid;
      margin-bottom: 1rem;
      border-color: ${(props) => props.theme.css.bkQuaternary};
      background-color: ${(props) => props.theme.css.bkQuaternary};
      border-radius: 1rem;
      padding: 1rem;
      .choose-filter {
        margin-left: auto;
        margin-right: auto;
      }
    }
    .add-schedule-btn {
      background-color: transparent;
      color: ${(props) => props.theme.css.btnBkPrimary};
      border: solid 2px;
    }
    .add-schedule {
      label {
        font-weight: bold;
      }
      .btn-clear {
        background-color: transparent;
        color: ${(props) => props.theme.css.btnBkPrimary};
        border: solid 2px;
        border-color: ${(props) => props.theme.css.btnBkPrimary};
      }
      display: flex;
      border: 1px solid;
      margin-bottom: 1rem;
      border-color: ${(props) => props.theme.css.bkQuaternary};
      background-color: ${(props) => props.theme.css.bkQuaternary};
      border-radius: 1rem;
      padding: 1rem;
      button {
        margin-left: auto;
      }
      .checkboxes {
        margin-left: 2rem;
        min-width: 10rem;
      }
      .schedule-content {
        .btn-clear {
          margin-left: 1rem;
        }
        margin-top: 2rem;
        .set-days {
          max-width: 10rem;
          margin-right: 3rem;
        }
      }
    }
  }
`;
