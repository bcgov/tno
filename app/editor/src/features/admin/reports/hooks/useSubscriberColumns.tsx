import { useFormikContext } from 'formik';
import {
  CellEllipsis,
  Checkbox,
  FormikSelect,
  getEnumStringOptions,
  IReportModel,
  ITableHookColumn,
  IUserReportModel,
  OptionItem,
  ReportDistributionFormatName,
  Row,
} from 'tno-core';

export const useSubscriberColumns = (): ITableHookColumn<IUserReportModel>[] => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const formatOptions = getEnumStringOptions(ReportDistributionFormatName);

  return [
    {
      label: 'Subscribed',
      accessor: 'id',
      width: '120px',
      hAlign: 'center',
      cell: (cell) => (
        <Checkbox
          id={`user-${cell.original.userId}`}
          value={true}
          checked={values.subscribers.some(
            (u) => u.userId === cell.original.userId && u.isSubscribed,
          )}
          onChange={(e) => {
            if (values.subscribers.some((u) => u.userId === cell.original.userId)) {
              const user = { ...cell.original, isSubscribed: e.target.checked };
              setFieldValue(
                'subscribers',
                values.subscribers.map((item) =>
                  item.userId === cell.original.userId ? user : item,
                ),
              );
            } else {
              const user = {
                ...cell.original,
                isSubscribed: e.target.checked,
                format: ReportDistributionFormatName.LinkOnly,
              };
              setFieldValue('subscribers', [user, ...values.subscribers]);
            }
          }}
        />
      ),
    },
    {
      label: 'Format',
      accessor: 'format',
      width: 1,
      cell: (cell) => (
        <FormikSelect
          name={`subscribers.${cell.row.index}.format`}
          options={formatOptions}
          value={formatOptions.find((o) => o.value === cell.original.format) ?? ''}
          onChange={(e) => {
            const option = e as OptionItem;
            if (option) {
              if (values.subscribers.some((u) => u.userId === cell.original.userId)) {
                const user = { ...cell.original, format: option.value };
                setFieldValue(
                  'subscribers',
                  values.subscribers.map((item) =>
                    item.userId === cell.original.userId ? user : item,
                  ),
                );
              } else {
                const user = {
                  ...cell.original,
                  isSubscribed: true,
                  format: option.value,
                };
                setFieldValue('subscribers', [user, ...values.subscribers]);
              }
            }
          }}
          isClearable={false}
        />
      ),
    },
    {
      label: 'Username',
      accessor: 'username',
      width: 1,
      cell: (cell) => <CellEllipsis>{cell.original.username}</CellEllipsis>,
    },
    {
      label: 'Last Name',
      accessor: 'lastName',
      width: 1,
      cell: (cell) => <CellEllipsis>{cell.original.lastName}</CellEllipsis>,
    },
    {
      label: 'First Name',
      accessor: 'firstName',
      width: 1,
      cell: (cell) => <CellEllipsis>{cell.original.firstName}</CellEllipsis>,
    },
    {
      label: 'Email',
      accessor: 'email',
      width: 2,
      cell: (cell) => (
        <Row gap="0.15rem">
          <CellEllipsis>{cell.original.email}</CellEllipsis>
          {cell.original.preferredEmail && (
            <CellEllipsis className="preferred">{cell.original.preferredEmail}</CellEllipsis>
          )}
        </Row>
      ),
    },
  ];
};
