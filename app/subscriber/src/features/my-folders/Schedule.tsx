import { TimeInput } from 'components/form/timeinput';
import moment from 'moment';
import * as React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaInfoCircle } from 'react-icons/fa';
import { FaGear } from 'react-icons/fa6';
import { Tooltip } from 'react-tooltip';
import { Checkbox, Col, FieldSize, IFolderScheduleModel, Row, Show, Text } from 'tno-core';

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
      <Row>
        <FaGear className="small-gear" /> <h3>Schedule content removal</h3>
      </Row>
      <div className="add-schedule">
        <span className="schedule-text">
          Configure when you would like to have content removed automatically.
        </span>
        <Row alignSelf="center" className="schedule-content">
          <Col>
            <Checkbox
              label="Scheduled content removal is enabled"
              name="schedule-enabled"
              checked={!!folderSchedule && !!folderSchedule.isEnabled}
              onChange={(e) => {
                onScheduleChange?.({
                  ...folderSchedule,
                  isEnabled: e.target.checked,
                } as IFolderScheduleModel);
              }}
            />
            <div className="main-sched-body">
              <Row className="time-inputs">
                <Col>
                  <TimeInput
                    label="Run schedule at:"
                    placeholder="hh:mm:ss"
                    value={!!folderSchedule ? folderSchedule?.startAt : ''}
                    width={FieldSize.Small}
                    onChange={(e) => {
                      onScheduleChange?.({
                        ...folderSchedule,
                        startAt: e.target.value,
                      } as IFolderScheduleModel);
                    }}
                  />
                </Col>
                <Col className="start-after-col">
                  <label>Run at:</label>
                  <ReactDatePicker
                    minDate={new Date()}
                    showMonthDropdown
                    showYearDropdown
                    showPreviousMonths
                    showTimeInput
                    selected={
                      folderSchedule && folderSchedule.runOn
                        ? new Date(folderSchedule.runOn)
                        : undefined
                    }
                    isClearable
                    showTimeSelect
                    dateFormat="MM/dd/yyyy HH:mm:ss"
                    value={
                      folderSchedule && folderSchedule.runOn
                        ? moment(folderSchedule.runOn).format('MM/DD/yyyy HH:mm:ss')
                        : ''
                    }
                    onChange={(date) => {
                      onScheduleChange?.({
                        ...folderSchedule,
                        runOn: moment(date).toISOString(),
                      } as IFolderScheduleModel);
                    }}
                  />
                </Col>
              </Row>
              <label>Choose days to run: </label>
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
              <Row className="keep-stories">
                Keep stories for
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
                days.
                <FaInfoCircle data-tooltip-id="keep-age" />
              </Row>
            </div>
          </Col>
          <Row>
            <Tooltip id="keep-age" variant="light">
              Remove content older than specified amount of days. Use '0' if you would like to
              remove all content.
            </Tooltip>
          </Row>
        </Row>
      </div>
    </Show>
  );
};
