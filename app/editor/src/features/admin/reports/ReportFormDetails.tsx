import { useFormikContext } from 'formik';
import { debounce, noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useApp } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
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
  const { values, setFieldValue, setValues } = useFormikContext<IReportModel>();
  const [{ userInfo }] = useApp();
  const [{ users }, { findUsers }] = useUsers();

  const [userOptions, setUserOptions] = React.useState(getUserOptions(users.items));

  React.useEffect(() => {
    if (userInfo?.id) {
      findUsers({ quantity: 50, includeUserId: values.ownerId })
        .then((results) => {
          setUserOptions(getUserOptions(results.items));
        })
        .catch(() => {});
    }
    // Only fire on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo?.id]);

  const handleFindUsers = debounce(async (text: string) => {
    const results = await findUsers({ quantity: 50, keyword: text }, true);
    setUserOptions(getUserOptions(results.items));
    return results;
  }, 500);

  const handleReassignOwnership = React.useCallback(() => {
    const report = {
      ...values,
      sections: values.sections.map((section) => ({
        ...section,
        filter: section.filter ? { ...section.filter, ownerId: values.ownerId } : section.filter,
        folder: section.folder
          ? {
              ...section.folder,
              ownerId: values.ownerId,
              filter: section.folder.filter
                ? { ...section.folder.filter, ownerId: values.ownerId }
                : section.folder.filter,
            }
          : section.folder,
      })),
    };
    setValues(report);
    console.debug(report.sections);
  }, [setValues, values]);

  return (
    <>
      <Col className="form-inputs">
        <FormikText
          name="name"
          label="Name"
          onChange={(e) => {
            const name = e.target.value;
            if (!values.settings.subject.text || values.settings.subject.text === values.name)
              setFieldValue('settings.subject.text', name);
            setFieldValue('name', name);
            if (values.templateId === 0)
              setFieldValue('template.name', `${name}-${Date.now().toString()}`);
          }}
          placeholder="Enter unique report name"
        />
        <FormikTextArea name="description" label="Description" />
        <Row alignItems="end">
          <FormikSelect
            name="ownerId"
            label="Owner"
            options={userOptions}
            value={userOptions.find((u) => u.value === values.ownerId) || ''}
            width="50ch"
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
          <Col className="frm-in">
            <Button variant={ButtonVariant.secondary} onClick={() => handleReassignOwnership()}>
              Apply ownership to filters/folders in report
            </Button>
          </Col>
        </Row>
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
        <Row alignItems="center">
          <FormikCheckbox label="Is Public" name="isPublic" />
          <p>
            A public report is available for all users. If they subscribe to the report they will
            receive a copy every time it is run.
          </p>
        </Row>
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
