import { useFormikContext } from 'formik';
import { FaTrash } from 'react-icons/fa';
import { Button, ButtonVariant, Col, FormikSelect, FormikText, Row } from 'tno-core';

import { labelOptions } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';

/**
 * Provides a way to view/edit content labels.
 * @returns the ContentLabelsForm
 */
export const ContentLabelsForm: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();

  const handleDelete = (id: number, key: string, value: string) => {
    setFieldValue(
      'labels',
      values.labels.filter((l) => !(l.id === id && l.key === key && l.value === value)),
    );
  };

  const labels = values.labels.map((l, i) => {
    return (
      <Row key={`${i}-${l.id}`}>
        <Col flex="0.5 0.5 0">
          <FormikSelect
            name={`labels.${1}.key`}
            options={labelOptions}
            value={labelOptions.find((l) => l.value === values.labels[i].key)}
          />
        </Col>
        <Col flex="2 1.5 0">
          <FormikText name={`labels.${i}.value`} />
        </Col>
        <Col>
          <Button variant={ButtonVariant.danger} onClick={() => handleDelete(l.id, l.key, l.value)}>
            <FaTrash />
          </Button>
        </Col>
      </Row>
    );
  });
  return <styled.ContentLabelsForm>{labels}</styled.ContentLabelsForm>;
};
