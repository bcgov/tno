import { FormikForm } from 'components/formik';
import { SearchWithLogout } from 'components/search-with-logout';
import { FormikProps } from 'formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useReports, useReportTemplates } from 'store/hooks';
import { useAppStore, useProfileStore } from 'store/slices';
import { Col, Container, Row } from 'tno-core';

import { defaultReport } from '../../constants';
import { IReportForm } from '../../interfaces';
import { toForm } from '../../utils';
import { ReportFormSchema } from '../validation/ReportFormSchema';
import { ReportAdminEdit } from './ReportAdminEdit';
import { ReportAdminPreview } from './ReportAdminPreview';
import * as styled from './styled';

const loading = ['get-report', 'add-report', 'update-report', 'delete-report'];

export interface IReportAdminProps {
  path?: string;
}

/**
 * Component provides a way to configure a report.
 * Two column page to show admin tools on left, and preview on right.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportAdmin: React.FC<IReportAdminProps> = ({ path: defaultPath = 'settings' }) => {
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const { id, path = defaultPath } = useParams();
  const [{ myReports }] = useProfileStore();
  const [{ getReport, addReport, updateReport, findMyReports }] = useReports();
  const [{ getReportTemplates }] = useReportTemplates();
  const [{ requests }] = useAppStore();

  const [report, setReport] = React.useState<IReportForm>(defaultReport(userInfo?.id ?? 0, 0));

  React.useEffect(() => {
    if (!myReports.length) {
      findMyReports().catch(() => {});
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    if (reportId)
      getReport(reportId)
        .then((result) => {
          if (result) {
            setReport(toForm(result));
          }
        })
        .catch(() => {});
    // Only make a request when 'id' changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  React.useEffect(() => {
    if (userInfo) setReport((report) => ({ ...report, ownerId: userInfo.id }));
  }, [userInfo]);

  React.useEffect(() => {
    getReportTemplates()
      .then((templates) => {
        // TODO: Support multiple public report templates.
        if (templates.length) {
          const template = templates[0];
          setReport((report) => ({ ...report, templateId: template.id, template: template }));
        } else toast.error('There are no public report templates available.');
      })
      .catch(() => {});
  }, [getReportTemplates]);

  const handleSubmit = React.useCallback(
    async (values: IReportForm) => {
      try {
        const originalId = values.id;
        const sameNameReport = myReports.some(
          (r) => r.name.trim().toLocaleLowerCase() === values.name.trim().toLocaleLowerCase(),
        );
        if (sameNameReport && !originalId) {
          toast.error(`A report with the name '${values.name}' already exists.`);
        } else {
          const report = originalId ? await updateReport(values) : await addReport(values);

          if (!originalId) {
            navigate(`/reports/${report.id}${path ? `/${path}` : ''}`);
            toast.success(`Successfully created '${report.name}'.`);
          } else {
            setReport(toForm(report));
            toast.success(`Successfully updated '${report.name}'.`);
          }
        }
      } catch {}
    },
    [addReport, myReports, navigate, path, updateReport],
  );

  return (
    <styled.ReportAdmin>
      <SearchWithLogout />
      <FormikForm
        initialValues={report}
        validationSchema={ReportFormSchema}
        validateOnChange={false}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {(props: FormikProps<IReportForm>) => (
          <Row className="report-layout" gap="1rem">
            <Col flex="1" className="edit">
              <Container isLoading={requests.some((r) => loading.includes(r.url))}>
                <ReportAdminEdit />
              </Container>
            </Col>
            <Col flex="1" className="preview">
              <ReportAdminPreview />
            </Col>
          </Row>
        )}
      </FormikForm>
    </styled.ReportAdmin>
  );
};
