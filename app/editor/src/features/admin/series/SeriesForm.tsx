import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { FaExclamationCircle } from 'react-icons/fa';
import { Outlet, useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useSeries } from 'store/hooks/admin';
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
  getSourceOptions,
  IconButton,
  IOptionItem,
  Modal,
  OptionItem,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultSeries } from './constants';
import { ISeriesForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

/** The page used to view and edit series the administrative section. */
const SeriesForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const targetSeriesId = Number(id);

  return (
    <styled.SeriesForm>
      <Row>
        <Col flex="2 1">
          <IconButton
            iconType="back"
            label="Back to Show/Programs"
            className="back-button"
            onClick={() => navigate('/admin/programs')}
          />
          <Tabs
            tabs={
              <>
                <Tab navigateTo="details" label="Details" exact activePaths={[`${id}`]} />
                <Tab navigateTo="merge" label="Merge" disabled={targetSeriesId === 0} />
              </>
            }
          >
            <Outlet />
          </Tabs>
        </Col>
      </Row>
    </styled.SeriesForm>
  );
};

export default SeriesForm;
