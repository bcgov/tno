import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IConnectionModel } from 'hooks';
import { Col, Row } from 'tno-core';

export const HTTPConfiguration = () => {
  const { values } = useFormikContext<IConnectionModel>();

  return (
    <div>
      <FormikText
        label="URL"
        name="configuration.url"
        value={values.configuration?.url}
        type="url"
        required
      />
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Username"
            name="configuration.username"
            value={values.configuration?.username}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Password"
            name="configuration.password"
            value={values.configuration?.password}
            type="password"
            autoComplete="off"
          />
        </Col>
      </Row>
    </div>
  );
};
