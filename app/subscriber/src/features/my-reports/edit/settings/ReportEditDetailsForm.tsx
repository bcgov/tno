import { FaInfoCircle } from 'react-icons/fa';
import { FormikText, FormikTextArea } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

export const ReportEditDetailsForm = () => {
  const { setFieldValue } = useReportEditContext();

  return (
    <styled.ReportEditDetailsForm>
      <div className="info">
        <div>
          <FaInfoCircle />
          <label>Report info</label>
        </div>
        <p>The report name here will become the title of the report.</p>
        <p>
          The description is optional, but can help you identify this report from others you may
          also own. The description will only show on your My Reports dashboard.
        </p>
      </div>
      <div>
        <p>Name your report and provide a description that will help you identify it.</p>
        <FormikText
          name="name"
          label="Report Name:"
          required
          placeholder="Enter unique report name"
          onChange={(e) => {
            setFieldValue('name', e.target.value);
          }}
        />
        <FormikTextArea name="description" label="Description:" />
      </div>
    </styled.ReportEditDetailsForm>
  );
};
