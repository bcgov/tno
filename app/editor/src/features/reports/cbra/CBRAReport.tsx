import React from 'react';
import {
  Button,
  ButtonVariant,
  FieldSize,
  IOptionItem,
  Select,
  SelectDate,
  useApiReports,
} from 'tno-core';

import { defaultValues, reportDurations } from './constants';
import { ICBRAForm } from './interfaces';
import * as styled from './styled';
import { calcDuration } from './utils';

interface ICBRAReportProps extends React.HTMLAttributes<HTMLDivElement> {}

export const CBRAReport: React.FC<ICBRAReportProps> = ({ className }) => {
  const api = useApiReports();

  const [values, setValues] = React.useState<ICBRAForm>({ ...defaultValues, ...calcDuration(0) });
  const [isDownloading, setIsDownloading] = React.useState(false);
  const isCustomRange = values.duration === 4;

  return (
    <styled.CBRAReport className={`${className ?? 'form'}`}>
      <div className="report-form">
        <Select
          name="duration"
          label="Report Duration"
          options={reportDurations}
          defaultValue={reportDurations[0]}
          onChange={(value) => {
            const option = (value as IOptionItem<number>).value ?? 0;
            setValues({
              duration: option,
              ...calcDuration(option),
            });
          }}
        />
        <div className="dates">
          <SelectDate
            name="start"
            label="Report Duration Start"
            placeholderText="YYYY MM DD"
            onChange={(date) => setValues({ ...values, start: date })}
            disabled={!isCustomRange}
            selected={values.start}
            width={FieldSize.Small}
          />
          {values.duration > 1 && (
            <SelectDate
              name="end"
              label="Report Duration End"
              placeholderText="YYYY MM DD"
              onChange={(date) => setValues({ ...values, end: date })}
              disabled={!isCustomRange}
              selected={values.end}
              width={FieldSize.Small}
            />
          )}
        </div>
        <div className="buttons">
          <Button
            disabled={!values.start || isDownloading}
            loading={isDownloading}
            onClick={async () => {
              if (values.start) {
                try {
                  setIsDownloading(true);
                  await api.generateCBRAReport(values.start, values.end);
                } finally {
                  setIsDownloading(false);
                }
              }
            }}
          >
            Generate CBRA Report
          </Button>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => setValues({ ...values, ...calcDuration(values.duration) })}
          >
            Clear
          </Button>
        </div>
      </div>
    </styled.CBRAReport>
  );
};
