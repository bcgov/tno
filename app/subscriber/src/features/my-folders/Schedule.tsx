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
  IFolderModel,
  IFolderScheduleModel,
  Row,
  Show,
  Text,
  TimeInput,
} from 'tno-core';

import { daysOfWeek } from './constants/daysOfWeek';

export interface IScheduleProps {
  setCurrentFolder: React.Dispatch<React.SetStateAction<IFolderModel | undefined>>;
  folderEvents?: IFolderScheduleModel[];
}
/*
 * This component will display the schedule options for clearing a folder in the ConfigureFolder component.
 */
export const Schedule: React.FC<IScheduleProps> = ({ folderEvents, setCurrentFolder }) => {
  const [days, setDays] = React.useState<string[]>([]);

  // init days
  React.useEffect(() => {
    if (folderEvents?.length && folderEvents[0].runOnWeekDays) {
      setDays(folderEvents[0].runOnWeekDays.split(','));
    }
    // only run to sync up with db
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [folderEvents?.length]);

  // easy way to convert checked days to a comma separated string
  React.useEffect(() => {
    if (!!days.length) {
      setCurrentFolder((prev) => {
        if (prev) {
          return { ...prev, events: [{ ...prev.events[0], runOnWeekDays: days.join(',') }] };
        }
      });
    }
    // only run when days change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [days]);

  return (
    <Show visible={!!folderEvents?.length}>
      <Row alignSelf="center" className="schedule-content">
        <Col>
          <Checkbox
            label="Schedule is enabled"
            checked={!!folderEvents && !!folderEvents[0]?.isEnabled}
            onChange={(e) => {
              setCurrentFolder((prev) => {
                if (prev) {
                  prev.events[0].isEnabled = e.target.checked;
                }
                return prev;
              });
            }}
          />
          <TimeInput
            label="Run schedle at:"
            placeholder='e.g. "13:00:00"'
            value={folderEvents && folderEvents[0]?.startAt}
            width={FieldSize.Small}
            onChange={(e) => {
              setCurrentFolder((prev) => {
                if (prev) {
                  prev.events[0].startAt = e.target.value;
                }
                return prev;
              });
            }}
          />
          <label>Start after:</label>
          <ReactDatePicker
            showTimeInput
            selected={
              folderEvents?.length && folderEvents[0].runOn
                ? new Date(folderEvents[0].runOn)
                : undefined
            }
            isClearable
            showTimeSelect
            dateFormat="MM/dd/yyyy HH:mm:ss"
            value={
              folderEvents?.length && folderEvents[0].runOn
                ? moment(folderEvents[0].runOn).format('MM/DD/yyyy HH:mm:ss')
                : undefined
            }
            onChange={(date) => {
              setCurrentFolder((prev) => {
                if (prev) {
                  return {
                    ...prev,
                    events: [{ ...prev.events[0], runOn: moment(date).toISOString() }],
                  };
                }
              });
            }}
          />
        </Col>
        <Col className="checkboxes">
          {daysOfWeek.map((day) => (
            <Checkbox
              key={day.value}
              label={day.label}
              checked={
                !!folderEvents && !!folderEvents[0]?.runOnWeekDays?.includes(day.value as string)
              }
              onChange={(e) => {
                if (e.target.checked) {
                  setDays([...days, e.target.value]);
                } else {
                  setDays(days.filter((day) => day !== e.target.value));
                }
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
            <Text width={FieldSize.Small} name="days" type="number" min="0" max="365" />
          </Row>
        </Col>
        <Col>
          <Text
            name="request-date"
            value={
              folderEvents?.length && folderEvents[0].requestSentOn
                ? folderEvents[0]?.requestSentOn
                : ''
            }
            label="Last Request Sent On"
            disabled
          >
            <Button
              tooltip="Clear last request sent on"
              className="btn-clear"
              onClick={() => {
                setCurrentFolder((prev) => {
                  if (prev) {
                    prev.events[0].requestSentOn = undefined;
                  }
                  return prev;
                });
              }}
            >
              <FaTrash />
            </Button>
          </Text>
          <Text
            name={`lastRanOn`}
            value={
              !!folderEvents?.length && folderEvents[0].lastRanOn ? folderEvents[0]?.lastRanOn : ''
            }
            label="Last Ran On"
            disabled
          />
        </Col>
      </Row>
    </Show>
  );
};
