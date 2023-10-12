import { useFormikContext } from 'formik';
import React from 'react';
import { toast } from 'react-toastify';
import { useContent } from 'store/hooks';
import { useFilters } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikStringEnumCheckbox,
  FormikText,
  FormikTimeInput,
  getDistinct,
  IContentModel,
  IContentRowModel,
  IFilterModel,
  OptionItem,
  Overlay,
  Row,
  ScheduleWeekDayName,
  Show,
  sortObject,
} from 'tno-core';

import { IFolderForm } from './interfaces';
import { createSchedule, getFilterOptions } from './utils';

export const FolderFormCollect: React.FC = () => {
  const { values, setFieldValue, setValues } = useFormikContext<IFolderForm>();
  const [{ filters }, { findAllFilters }] = useFilters();
  const [, { findContentWithElasticsearch }] = useContent();

  const [filterOptions, setFilterOptions] = React.useState(getFilterOptions(filters));
  const [filter, setFilter] = React.useState<IFilterModel>();

  React.useEffect(() => {
    if (!filters.length) {
      findAllFilters()
        .then((filters) => {
          setFilterOptions(getFilterOptions(filters));
        })
        .catch(() => {});
    }
  }, [filters, findAllFilters]);

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
        <Overlay title="Not Implemented Yet" />
        <Row gap="1rem">
          <p>Schedule when this folder will have content removed automatically.</p>
          <Show visible={values.schedule === undefined}>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() =>
                setValues({
                  ...values,
                  schedule: createSchedule(values.name, values.description),
                })
              }
            >
              Add Schedule
            </Button>
          </Show>
        </Row>
        <Show visible={values.schedule !== undefined}>
          <Row gap="1rem">
            <Col gap="1rem">
              <FormikCheckbox name={`schedule.isEnabled`} label="Enabled" value={true} />
              <FormikTimeInput
                name={`schedule.startAt`}
                label="Run At"
                width="7em"
                placeholder="HH:MM:SS"
              />
            </Col>
            <Col className="frm-in">
              <label>Run On</label>
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Monday"
                name={`schedule.runOnWeekDays`}
                value={ScheduleWeekDayName.Monday}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Tuesday"
                name={`schedule.runOnWeekDays`}
                value={ScheduleWeekDayName.Tuesday}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Wednesday"
                name={`schedule.runOnWeekDays`}
                value={ScheduleWeekDayName.Wednesday}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Thursday"
                name={`schedule.runOnWeekDays`}
                value={ScheduleWeekDayName.Thursday}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Friday"
                name={`schedule.runOnWeekDays`}
                value={ScheduleWeekDayName.Friday}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Saturday"
                name={`schedule.runOnWeekDays`}
                value={ScheduleWeekDayName.Saturday}
              />
              <FormikStringEnumCheckbox<ScheduleWeekDayName>
                label="Sunday"
                name={`schedule.runOnWeekDays`}
                value={ScheduleWeekDayName.Sunday}
              />
            </Col>
            <Col>
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
          </Row>
        </Show>
      </Col>
    </Col>
  );
};
