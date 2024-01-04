import moment from 'moment';
import * as React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaInfoCircle } from 'react-icons/fa';
import { FaTrash } from 'react-icons/fa6';
import { Tooltip } from 'react-tooltip';
import {
  Button,
  Checkbox,
  Col,
  FieldSize,
  IFolderScheduleModel,
  Row,
  Show,
  Text,
  TimeInput,
} from 'tno-core';

import { daysOfWeek } from './constants/daysOfWeek';

export interface IScheduleProps {
  folderSchedule?: IFolderScheduleModel;
  onScheduleChange?: (value: IFolderScheduleModel) => void;
}
/*
 * This component will display the schedule options for clearing a folder in the ConfigureFolder component.
 */
export const Schedule: React.FC<IScheduleProps> = ({ folderSchedule, onScheduleChange }) => {
  return (
    <Show visible={!!folderSchedule}>
      <Row alignSelf="center" className="schedule-content">
        <Col>
          <Checkbox
            label="Schedule is enabled"
            checked={!!folderSchedule && !!folderSchedule.isEnabled}
            onChange={(e) => {
              onScheduleChange?.({
                ...folderSchedule,
                isEnabled: e.target.checked,
              } as IFolderScheduleModel);
            }}
          />
          <TimeInput
            label="Run schedule at:"
            placeholder='e.g. "13:00:00"'
            value={folderSchedule && folderSchedule?.startAt}
            width={FieldSize.Small}
            onChange={(e) => {
              onScheduleChange?.({
                ...folderSchedule,
                startAt: e.target.value,
              } as IFolderScheduleModel);
            }}
          />
          <label>Start after:</label>
          <ReactDatePicker
            showTimeInput
            selected={
              folderSchedule && folderSchedule.runOn ? new Date(folderSchedule.runOn) : undefined
            }
            isClearable
            showTimeSelect
            dateFormat="MM/dd/yyyy HH:mm:ss"
            value={
              folderSchedule && folderSchedule.runOn
                ? moment(folderSchedule.runOn).format('MM/DD/yyyy HH:mm:ss')
                : undefined
            }
            onChange={(date) => {
              onScheduleChange?.({
                ...folderSchedule,
                runOn: moment(date).toISOString(),
              } as IFolderScheduleModel);
            }}
          />
        </Col>
        <Col className="checkboxes">
          {daysOfWeek.map((day) => (
            <Checkbox
              key={day.value}
              label={day.label}
              checked={folderSchedule?.runOnWeekDays.includes(day.value as string)}
              onChange={(e) => {
                let currentDays: string[] =
                  folderSchedule?.runOnWeekDays.replace(/\s/g, '').split(',') ?? [];
                let updatedDays: string[];
                if (e.target.checked) {
                  updatedDays = [...currentDays, e.target.value];
                } else {
                  updatedDays = currentDays.filter((day) => day !== e.target.value);
                }
                onScheduleChange?.({
                  ...folderSchedule,
                  runOnWeekDays: updatedDays.join(','),
                } as IFolderScheduleModel);
              }}
              value={day.value}
            />
          ))}
        </Col>
        <Col className="set-days">
          <Row>
            <Tooltip id="keep-age" variant="light">
              Remove content older than specified amount of days. Use '0' if you would like to
              remove all content.
            </Tooltip>
            <label>
              Keep age limit (days)
              <FaInfoCircle data-tooltip-id="keep-age" />
            </label>
            <Text
              width={FieldSize.Small}
              name="days"
              type="number"
              min="0"
              max="365"
              value={folderSchedule?.settings.keepAgeLimit ?? 0}
              onChange={(e) => {
                onScheduleChange?.({
                  ...folderSchedule,
                  settings: { keepAgeLimit: e.target.value },
                } as IFolderScheduleModel);
              }}
            />
          </Row>
        </Col>
        <Col>
          <Text
            name="request-date"
            value={
              folderSchedule && folderSchedule.requestSentOn ? folderSchedule?.requestSentOn : ''
            }
            label="Last Request Sent On"
            disabled
          >
            <Button
              tooltip="Clear last request sent on"
              className="btn-clear"
              onClick={() => {
                onScheduleChange?.({
                  ...folderSchedule,
                  requestSentOn: undefined,
                } as IFolderScheduleModel);
              }}
            >
              <FaTrash />
            </Button>
          </Text>
          <Text
            name={`lastRanOn`}
            value={!!folderSchedule && folderSchedule.lastRanOn ? folderSchedule?.lastRanOn : ''}
            label="Last Ran On"
            disabled
          />
        </Col>
      </Row>
    </Show>
  );
};
