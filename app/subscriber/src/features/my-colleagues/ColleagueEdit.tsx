import { FormikForm } from 'components/formik';
import React from 'react';
import { useParams } from 'react-router-dom';
import {
  Col,
  Container,
  FormikSelect,
  IColleagueModel,
  IOptionItem,
  IUserModel,
  OptionItem,
  Row,
} from 'tno-core';

import * as styled from './styled';

export const ColleagueEdit: React.FC = () => {
  const { id } = useParams();

  const [colleague, setColleague] = React.useState<IColleagueModel>();
  const [users, setUsers] = React.useState<IUserModel[]>([]);
  const [options, setOptions] = React.useState<IOptionItem[]>([new OptionItem('test', 'test')]);

  React.useEffect(() => {
    const colleagueId = parseInt(id ?? '0');
    // if (!!colleagueId) {
    //   generateReport(colleagueId)
    //     .then((result) => {
    //       if (result) setColleague(result);
    //     })
    //     .catch(() => {});
    // }
    // Only make a request if the 'id' changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  // const handleSubmit = React.useCallback(
  //   async (values: IReportForm) => {
  //     try {
  //       const result = await updateReport(values, true);
  //       setReport(toForm(result, report, true));
  //     } catch {}
  //   },
  //   [report, updateReport],
  // );

  return (
    <styled.MyColleagues>
      <FormikForm
        loading={false}
        initialValues={colleague}
        onSubmit={async (values, { setSubmitting }) => {
          // await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values }) => (
          <Row gap="1rem">
            <Col className="edit">
              <Container>
                <FormikSelect
                  name={`colleagueId`}
                  label="Colleague"
                  options={options}
                  value={options.find((o) => o.value === colleague?.colleagueId) ?? ''}
                  onChange={(newValue: any) => {
                    const option = newValue as OptionItem;
                    const filter = users.find((f) => f.id === option?.value);
                    // if (filter) setFieldValue(`sections.${index}.filter`, filter);
                  }}
                />
              </Container>
            </Col>
          </Row>
        )}
      </FormikForm>
    </styled.MyColleagues>
  );
};
