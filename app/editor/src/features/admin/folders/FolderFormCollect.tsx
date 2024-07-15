import { IContentRowModel } from 'components/content';
import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaEraser } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useContent } from 'store/hooks';
import { useFilters } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikStringEnumCheckbox,
  FormikText,
  FormikTimeInput,
  getDistinct,
  IContentModel,
  IFilterModel,
  OptionItem,
  Row,
  ScheduleWeekDayName,
  selectWeekDays,
  Show,
  sortObject,
} from 'tno-core';

import { IFolderForm } from './interfaces';
import { createSchedule, getFilterOptions } from './utils';

export const FolderFormCollect: React.FC = () => {
  const { values, setFieldValue, setValues } = useFormikContext<IFolderForm>();
  const [{ filters }, { findFilters }] = useFilters();
  const [, { findContentWithElasticsearch }] = useContent();

  const [filterOptions, setFilterOptions] = React.useState(
    getFilterOptions(filters, values.filterId),
  );
  const [filter, setFilter] = React.useState<IFilterModel>();

  React.useEffect(() => {
    if (!filters.length) {
      findFilters({})
        .then((filters) => {
          setFilterOptions(getFilterOptions(filters, values.filterId));
        })
        .catch(() => {});
    }
  }, [filters, findFilters, values.filterId]);

  const handleRun = React.useCallback(
    async (filter: IFilterModel) => {
      try {
        const results: any = await findContentWithElasticsearch(
          filter.query,
          filter.settings.searchUnpublished,
        );
        if (results.hits.hits.length) {
          const existing = [...values.content].sort(sortObject((c) => c.sortOrder));
          const maxSortOrder = existing.length ? existing[existing.length - 1].sortOrder : 0;
          const content: IContentRowModel[] = getDistinct(
            [
              ...existing,
              ...results.hits.hits.map((h: { _source: IContentModel }, index: number) => ({
                contentId: h._source.id,
                sortOrder: maxSortOrder + index,
                content: h._source,
                selected: false,
              })),
            ],
            (item) => item.content.id,
          ).map((item, index) => ({ ...item, sortOrder: index }));
          setFieldValue('content', content);
          toast.success(
            `Filter found ${results.hits.hits.length} content items.  Remember to save your folder if you want to keep them.`,
          );
        } else {
          toast.warning('No content found for this filter.');
        }
      } catch {}
    },
    [findContentWithElasticsearch, setFieldValue, values.content],
  );

  return (
    <Col gap="1rem">
      <Col>
        <p>Populate this folder automatically with the specified filter.</p>
        <FormikSelect
          name="filterId"
          label="Filter"
          value={filterOptions.find((o) => o.value === values.filterId)}
          options={filterOptions}
          onChange={(newValue) => {
            const option = newValue as OptionItem;
            setFilter(filters.find((f) => f.id === option?.value));
          }}
        >
          <Button
            variant={ButtonVariant.secondary}
            disabled={!values.filterId}
            onClick={() => handleRun(filters.find((f) => f.id === values.filterId)!)}
          >
            Run
          </Button>
        </FormikSelect>
        <Show visible={!!filter?.description}>
          <p className="note">{filter?.description}</p>
        </Show>
      </Col>
      <hr />
      <Col gap="1rem" style={{ position: 'relative' }}>
        <Row gap="1rem">
          <p>Schedule when this folder will have content removed automatically.</p>
          <Show visible={!values.events.length}>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() =>
                setValues({
                  ...values,
                  events: [createSchedule(values.name, values.description)],
                })
              }
            >
              Add Schedule
            </Button>
          </Show>
        </Row>
        <Show visible={!!values.events.length}>
          <Row gap="1rem">
            <Col gap="1rem">
              <FormikCheckbox name={`events.0.isEnabled`} label="Enabled" value={true} />
              <FormikTimeInput
                name={`events.0.startAt`}
                label="Run at"
                width="7em"
                placeholder="HH:MM:SS"
              />
              <FormikDatePicker
                name="events.0.runOn"
                label="Start after"
                width="13em"
                showTimeInput
                showTimeSelect
                dateFormat="MM/dd/yyyy HH:mm:ss"
                value={
                  values.events.length && values.events[0].runOn
                    ? moment(values.events[0].runOn).format('MM/DD/yyyy HH:mm:ss')
                    : undefined
                }
                onChange={(value) => {
                  setFieldValue('events.0.runOn', value ? moment(value).toString() : undefined);
                }}
                isClearable
              />
            </Col>
            <Col className="frm-in">
              <label>Weekdays</label>
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Monday"
                name={`events.0.runOnWeekDays`}
                value={ScheduleWeekDayName.Monday}
                onBeforeChange={(value) => selectWeekDays(value)}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Tuesday"
                name={`events.0.runOnWeekDays`}
                value={ScheduleWeekDayName.Tuesday}
                onBeforeChange={(value) => selectWeekDays(value)}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Wednesday"
                name={`events.0.runOnWeekDays`}
                value={ScheduleWeekDayName.Wednesday}
                onBeforeChange={(value) => selectWeekDays(value)}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Thursday"
                name={`events.0.runOnWeekDays`}
                value={ScheduleWeekDayName.Thursday}
                onBeforeChange={(value) => selectWeekDays(value)}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Friday"
                name={`events.0.runOnWeekDays`}
                value={ScheduleWeekDayName.Friday}
                onBeforeChange={(value) => selectWeekDays(value)}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Saturday"
                name={`events.0.runOnWeekDays`}
                value={ScheduleWeekDayName.Saturday}
                onBeforeChange={(value) => selectWeekDays(value)}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Sunday"
                name={`events.0.runOnWeekDays`}
                value={ScheduleWeekDayName.Sunday}
                onBeforeChange={(value) => selectWeekDays(value)}
              />
            </Col>
            <Col flex="1">
              <p>
                When content is automatically removed from the folder, only remove content that is
                older than the specified number of days. Use '0' to remove all content.
              </p>
              <Row>
                <FormikText
                  name="settings.keepAgeLimit"
                  label="Keep age limit (days)"
                  type="number"
                />
              </Row>
            </Col>
            <Col>
              <FormikText name={`events.0.requestSentOn`} label="Last Request Sent On" disabled>
                <Button
                  variant={ButtonVariant.danger}
                  tooltip="Clear last request sent on"
                  onClick={() => setFieldValue(`events.0.requestSentOn`, undefined)}
                >
                  <FaEraser />
                </Button>
              </FormikText>
              <FormikText name={`events.0.lastRanOn`} label="Last Ran On" disabled />
            </Col>
          </Row>
        </Show>
      </Col>
    </Col>
  );
};
