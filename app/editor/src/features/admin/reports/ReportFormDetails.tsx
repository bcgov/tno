import { useFormikContext } from 'formik';
import { debounce, noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useApp } from 'store/hooks';
import { useReports, useUsers } from 'store/hooks/admin';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  getUserOptions,
  IReportModel,
  OptionItem,
  Row,
  Show,
} from 'tno-core';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormDetails: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ reports }, { findAllReports }] = useReports();
  const [{ userInfo }] = useApp();
  const [{ users }, { findUsers }] = useUsers();

  const [userOptions, setUserOptions] = React.useState(getUserOptions(users.items));
  const [reportOptions, setReportOptions] = React.useState(getSortableOptions(reports));

  React.useEffect(() => {
    if (!reports.length)
      findAllReports()
        .then((reports) => {
          setReportOptions(getSortableOptions(reports));
        })
        .catch();
    // Only run on initialize.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (userInfo?.id) {
      findUsers({ quantity: 50, includeUserId: values.ownerId })
        .then((results) => {
          setUserOptions(getUserOptions(results.items));
        })
        .catch();
    }
    // Only fire on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo?.id]);

  const handleFindUsers = debounce(async (text: string) => {
    const results = await findUsers({ quantity: 50, username: text }, true);
    setUserOptions(getUserOptions(results.items));
    return results;
  }, 500);

  return (
    <>
      <Col className="form-inputs">
        <FormikText
          name="name"
          label="Name"
          required
          onChange={(e) => {
            setFieldValue('name', e.target.value);
            if (values.templateId === 0)
              setFieldValue('template.name', `${e.target.value}-${Date.now().toString()}`);
          }}
        />
        <FormikTextArea name="description" label="Description" />
        <FormikSelect
          name="ownerId"
          label="Owner"
          options={userOptions}
          value={userOptions.find((u) => u.value === values.ownerId) || ''}
          onChange={(e) => {
            const option = e as OptionItem;
            setFieldValue(
              'values.ownerId',
              option?.value ? parseInt(option.value.toString()) : undefined,
            );
          }}
          onInputChange={(newValue) => {
            // TODO: Does not need to fire when an option is manually selected.
            handleFindUsers(newValue);
          }}
        />
        <Row alignItems="center">
          <FormikText
            width={FieldSize.Tiny}
            name="sortOrder"
            label="Sort Order"
            type="number"
            className="sort-order"
          />
          <FormikCheckbox label="Is Enabled" name="isEnabled" />
        </Row>
        <Col className="frm-in">
          <label>Report Options</label>
          <Row alignItems="center">
            <FormikCheckbox label="Is Public" name="isPublic" />
            <p>
              A public report is available for all users. If they subscribe to the report they will
              receive a copy every time it is run.
            </p>
          </Row>
          <Row>
            <FormikCheckbox
              label="View On Web Only"
              name="settings.viewOnWebOnly"
              tooltip="Email will only contain a link to view the report on the website"
            />
            <FormikCheckbox
              label="Exclude Historical Content"
              name="settings.instance.excludeHistorical"
            />
          </Row>
          <Row>
            <FormikSelect
              name="settings.instance.excludeReports"
              label="Exclude Related Report Content"
              tooltip="Excludes content already reported on in the selected reports"
              options={reportOptions}
              isMulti
              value={reportOptions.filter((ro) =>
                values.settings.instance.excludeReports.some((reportId) => reportId === ro.value),
              )}
              onChange={(newValue) => {
                if (Array.isArray(newValue))
                  setFieldValue(
                    'settings.instance.excludeReports',
                    newValue.map((v: OptionItem) => v.value),
                  );
              }}
            />
          </Row>
        </Col>
        <Col className="frm-in">
          <label>Headline Options</label>
          <Row>
            <FormikCheckbox label="Show Source" name="settings.headline.showSource" />
            <FormikCheckbox label="Show Common Call" name="settings.headline.showShortName" />
            <FormikCheckbox label="Show Published On" name="settings.headline.showPublishedOn" />
            <FormikCheckbox label="Show Sentiment" name="settings.headline.showSentiment" />
          </Row>
        </Col>
        <Col className="frm-in">
          <label>Content Options</label>
          <Row>
            <FormikCheckbox label="Include Story" name="settings.content.includeStory" />
            <FormikCheckbox label="Show Images" name="settings.content.showImage" />
            <FormikCheckbox label="Use Thumbnails" name="settings.content.useThumbnail" />
            <FormikCheckbox label="Highlight Keywords" name="settings.content.highlightKeywords" />
          </Row>
        </Col>
        <hr />
        <Row>
          <Col>
            <Show visible={!!values.id}>
              <Row>
                <FormikText width={FieldSize.Small} disabled name="updatedBy" label="Updated By" />
                <FormikDatePicker
                  selectedDate={
                    !!values.updatedOn ? moment(values.updatedOn).toString() : undefined
                  }
                  onChange={noop}
                  name="updatedOn"
                  label="Updated On"
                  disabled
                  width={FieldSize.Small}
                />
              </Row>
              <Row>
                <FormikText width={FieldSize.Small} disabled name="createdBy" label="Created By" />
                <FormikDatePicker
                  selectedDate={
                    !!values.createdOn ? moment(values.createdOn).toString() : undefined
                  }
                  onChange={noop}
                  name="createdOn"
                  label="Created On"
                  disabled
                  width={FieldSize.Small}
                />
              </Row>
            </Show>
          </Col>
        </Row>
      </Col>
    </>
  );
};
