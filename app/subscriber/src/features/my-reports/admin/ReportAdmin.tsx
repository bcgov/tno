import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useReports, useReportTemplates } from 'store/hooks';
import { Col, Row } from 'tno-core';

import { defaultReport } from '../constants';
import { IReportForm } from '../interfaces';
import { getHideEmpty, toForm } from '../utils';
import { ReportAdminEdit } from './ReportAdminEdit';
import { ReportAdminPreview } from './ReportAdminPreview';
import * as styled from './styled';

export interface IReportAdminProps {
  path?: string;
}

/**
 * Component provides a way to configure a report.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportAdmin: React.FC<IReportAdminProps> = ({ path: defaultPath = 'settings' }) => {
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const { id, path = defaultPath } = useParams();
  const [{ getReport, addReport, updateReport }] = useReports();
  const [{ getReportTemplates }] = useReportTemplates();

  const [report, setReport] = React.useState<IReportForm>(defaultReport(userInfo?.id));

  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    if (reportId && !report.id)
      getReport(reportId)
        .then((result) => {
          if (result) setReport(toForm(result, report));
        })
        .catch(() => {});
  }, [getReport, id, report]);

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
      const originalId = values.id;
      const report = originalId ? await updateReport(values) : await addReport(values);

      if (!originalId) {
        navigate(`/reports/${report.id}${path ? `/${path}` : ''}`);
        toast.success(`Successfully created '${report.name}'.`);
      } else {
        setReport({
          ...report,
          sections: report.sections.map((section, index) => ({
            ...section,
            expand: values.sections[index].expand,
          })),
          hideEmptySections: getHideEmpty(report.sections),
        });
        toast.success(`Successfully updated '${report.name}'.`);
      }
    },
    [addReport, navigate, path, updateReport],
  );

  return (
    <styled.ReportAdmin>
      <FormikForm
        initialValues={report}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values }) => (
          <Row className="report-layout" gap="1rem">
            <Col flex="1" className="edit">
              <ReportAdminEdit />
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
