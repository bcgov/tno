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
  type IReportModel,
  type IUserModel,
  type OptionItem,
  Row,
  Show,
  UserAccountTypeName,
} from 'tno-core';

const accountTypes = [
  UserAccountTypeName.Direct,
  UserAccountTypeName.Indirect,
  UserAccountTypeName.SystemAccount,
];

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormDetails: React.FC = () => {
  const { values, setFieldValue, setValues } = useFormikContext<IReportModel>();
  const [{ userInfo }] = useApp();
  const [, { findUsers }] = useUsers();

  const [owners, setOwners] = React.useState<IUserModel[]>([]);
  const [isSearching, setIsSearching] = React.useState(false);
  const searchId = React.useRef(0);

  // The owner arrives with the report, so it displays without being in the search results.
  const ownerOption = React.useMemo(() => {
    if (!values.ownerId) return null;
    const owner =
      values.owner?.id === values.ownerId
        ? values.owner
        : owners.find((u) => u.id === values.ownerId) ??
          (userInfo?.id === values.ownerId ? (userInfo as unknown as IUserModel) : undefined);
    return owner ? getUserOptions([owner])[0] : null;
  }, [owners, userInfo, values.owner, values.ownerId]);

  const searchOwners = React.useMemo(
    () =>
      debounce(async (keyword: string) => {
        const id = ++searchId.current;
        try {
          const results = await findUsers({ quantity: 50, keyword, accountTypes }, true);
          if (id === searchId.current) setOwners(results.items);
        } catch {
        } finally {
          if (id === searchId.current) setIsSearching(false);
        }
      }, 500),
    [findUsers],
  );

  React.useEffect(() => () => searchOwners.cancel(), [searchOwners]);

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
  }, [setValues, values]);

  return (
    <>
      <Col className="form-inputs">
        <FormikText
          name="name"
          label="Name"
          onChange={(e) => {
            const name = e.target.value;
            if (!values.settings.subject.text || values.settings.subject.text === values.name) {
              setFieldValue('settings.subject.text', name);
            }
            setFieldValue('name', name);
            if (values.templateId === 0) {
              setFieldValue('template.name', `${name}-${Date.now().toString()}`);
            }
          }}
          placeholder="Enter unique report name"
        />
        <FormikTextArea name="description" label="Description" />
        <Row alignItems="end">
          <FormikSelect
            name="ownerId"
            label="Owner"
            placeholder="Search by name, username or email"
            options={getUserOptions(owners)}
            value={ownerOption}
            width="50ch"
            isLoading={isSearching}
            // The API matches the keyword, including on email, which the labels do not show.
            filterOption={null}
            noOptionsMessage={({ inputValue }) =>
              inputValue.trim() ? 'No users found' : 'Type to search for a user'
            }
            onChange={(e) => {
              const option = e as OptionItem | undefined;
              setFieldValue(
                'owner',
                owners.find((u) => u.id === option?.value),
              );
            }}
            onInputChange={(text, { action }) => {
              if (action !== 'input-change') return;
              if (text.trim()) {
                setIsSearching(true);
                searchOwners(text.trim());
              } else {
                searchOwners.cancel();
                searchId.current++;
                setIsSearching(false);
                setOwners([]);
              }
            }}
          />
          <Col className="frm-in">
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => {
                handleReassignOwnership();
              }}
            >
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
                  selectedDate={values.updatedOn ? moment(values.updatedOn).toString() : undefined}
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
                  selectedDate={values.createdOn ? moment(values.createdOn).toString() : undefined}
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
