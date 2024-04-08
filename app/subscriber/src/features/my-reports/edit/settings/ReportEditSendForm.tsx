import { FaInfoCircle } from 'react-icons/fa';
import { FormikCheckbox, FormikText } from 'tno-core';

import * as styled from './styled';
import { ReportSchedule } from './template';

export const ReportEditSendForm = () => {
  return (
    <styled.ReportEditSendForm>
      <h2>Email options</h2>
      <div>
        <FormikText name="settings.subject.text" label="Email subject line:" required />
        <FormikCheckbox
          name="settings.subject.showTodaysDate"
          label="Append the report date to the subject line"
        />
      </div>

      <h2>Scheduling</h2>
      <div className="info">
        <div>
          <FaInfoCircle /> Scheduling...
        </div>
        Scheduling a report will populate it and send to its subscribers automatically. Each report
        can have up to TWO schedules. The second schedule may be used for sending out reports at a
        different time on certain days.
      </div>

      <div className="schedules">
        <ReportSchedule index={0} label="Schedule 1" />
        <ReportSchedule index={1} label="Schedule 2" />
      </div>
    </styled.ReportEditSendForm>
  );
};
