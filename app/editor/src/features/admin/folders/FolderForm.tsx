import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useFolders } from 'store/hooks/admin';
import { Button, ButtonVariant, IconButton, Modal, Row, Show, Tab, Tabs, useModal } from 'tno-core';

import { defaultFolder } from './constants';
import { FolderFormCollect } from './FolderFormCollect';
import { FolderFormContent } from './FolderFormContent';
import { FolderFormDetails } from './FolderFormDetails';
import { IFolderForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

/**
 * Provides a page to admin a list of folders.
 * @returns Component to admin folders.
 */
export const FolderForm: React.FC = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { addFolder, deleteFolder, getFolder, updateFolder }] = useFolders();
  const { toggle, isShowing } = useModal();

  const [active, setActive] = React.useState('details');
  const [folder, setFolder] = React.useState<IFolderForm>({
    ...defaultFolder,
    ownerId: userInfo?.id ?? 0,
  });

  const folderId = Number(id);

  React.useEffect(() => {
    if (!!folderId && folder?.id !== folderId) {
      setFolder({ ...defaultFolder, id: folderId }); // Do this to stop double fetch.
      getFolder(folderId, true).then((data) => {
        setFolder(toForm(data));
      });
    }
  }, [getFolder, folder?.id, folderId]);

  const handleSubmit = async (values: IFolderForm) => {
    try {
      const originalId = values.id;
      const result = !folder.id
        ? await addFolder(toModel(values))
        : await updateFolder(toModel(values));
      setFolder(toForm(result));

      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/folders/${result.id}`);
    } catch {}
  };

  return (
    <styled.FolderForm>
      <IconButton
        iconType="back"
        label="Back to folders"
        className="back-button"
        onClick={() => navigate('/admin/folders')}
      />
      <FormikForm
        initialValues={folder}
        enableReinitialize={true}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <Tabs
            tabs={
              <>
                <Tab
                  label="Details"
                  onClick={() => {
                    setActive('details');
                  }}
                  active={active === 'details'}
                />
                <Tab
                  label="Collection"
                  onClick={() => {
                    setActive('collect');
                  }}
                  active={active === 'collect'}
                />
                <Tab
                  label="Content"
                  onClick={() => {
                    setActive('content');
                  }}
                  active={active === 'content'}
                />
                <Row justifyContent="flex-end" flex="1">
                  <Button
                    variant={ButtonVariant.link}
                    disabled={!values.content.length}
                    onClick={() => {
                      if (values.content.some((item) => item.selected)) {
                        setFieldValue(
                          'content',
                          values.content.filter((item) => !item.selected),
                        );
                      } else setFieldValue('content', []);
                    }}
                  >
                    Empty folder
                  </Button>
                </Row>
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'details'}>
                <FolderFormDetails />
              </Show>
              <Show visible={active === 'collect'}>
                <FolderFormCollect />
              </Show>
              <Show visible={active === 'content'}>
                <FolderFormContent />
              </Show>
              <Row className="form-actions">
                <Button type="submit" disabled={isSubmitting}>
                  Save
                </Button>
                <Show visible={!!values.id}>
                  <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                    Delete
                  </Button>
                </Show>
              </Row>
              <Modal
                headerText="Confirm Removal"
                body="Are you sure you wish to remove this folder?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    await deleteFolder(toModel(folder));
                    toast.success(`${folder.name} has successfully been deleted.`);
                    navigate('/admin/folders');
                  } catch {
                    // Globally handled
                  } finally {
                    toggle();
                  }
                }}
              />
            </div>
          </Tabs>
        )}
      </FormikForm>
    </styled.FolderForm>
  );
};

export default FolderForm;
