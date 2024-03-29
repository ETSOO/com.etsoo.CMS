import {
  CommonPage,
  FileUploadButton,
  HBox,
  InputField,
  VBox
} from '@etsoo/materialui';
import { DnDList, useParamsEx } from '@etsoo/react';
import {
  IconButton,
  ImageList,
  ImageListItem,
  ImageListItemBar,
  LinearProgress,
  Stack,
  Typography
} from '@mui/material';
import React from 'react';
import { useLocation } from 'react-router-dom';
import { ArticleLogoDto } from '../../api/dto/article/ArticleLogoDto';
import FileUploadIcon from '@mui/icons-material/FileUpload';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import DragIndicatorIcon from '@mui/icons-material/DragIndicator';
import { app } from '../../app/MyApp';
import { GalleryPhotoListItem } from '../../api/dto/article/GalleryPhotoDto';
import { DomUtils } from '@etsoo/shared';

function ArticleGallery() {
  // Route
  const { id = 0 } = useParamsEx({ id: 'number' });

  const location = useLocation();
  const state = location.state as ArticleLogoDto;

  // State
  const [photos, setPhotos] = React.useState<GalleryPhotoListItem[]>();

  // Labels
  const {
    articleTitle,
    photoUploadTip,
    photoUploadError1,
    photoUploadError2,
    photoUpload,
    delete: deleteLabel,
    deleteConfirm,
    dragIndicator,
    edit,
    title,
    description,
    link
  } = app.getLabels(
    'articleTitle',
    'photoUploadTip',
    'photoUploadError1',
    'photoUploadError2',
    'photoUpload',
    'delete',
    'deleteConfirm',
    'dragIndicator',
    'edit',
    'title',
    'description',
    'link'
  );

  const loadData = React.useCallback(async () => {
    const items = await app.articleApi.viewGallery(id, { defaultValue: [] });
    if (items == null) return;
    setPhotos(items.map((item, index) => ({ id: index, ...item })));
  }, [id]);

  const editPhoto = (photo: GalleryPhotoListItem) => {
    app.showInputDialog({
      title: edit,
      message: undefined,
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const { title, description, link } = DomUtils.dataAs(
          new FormData(form),
          {
            title: 'string',
            description: 'string',
            link: 'string'
          }
        );

        // Submit
        const result = await app.articleApi.updatePhoto(
          { id, url: photo.url, title, description, link },
          { showLoading: false }
        );
        if (result == null) return;

        if (result.ok) {
          loadData();
          return;
        }

        return app.formatResult(result);
      },
      inputs: (
        <VBox gap={2} marginTop={2}>
          <InputField
            fullWidth
            name="title"
            inputProps={{ maxLength: 128 }}
            label={title}
            defaultValue={photo.title}
          />
          <InputField
            fullWidth
            multiline
            rows={3}
            name="description"
            inputProps={{ maxLength: 1280 }}
            label={description}
            defaultValue={photo.description}
          />
          <InputField
            fullWidth
            name="link"
            inputProps={{ maxLength: 256 }}
            label={link}
            defaultValue={photo.link}
          />
        </VBox>
      )
    });
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('slideshowLogo');

    loadData();
  }, [loadData]);

  return (
    <CommonPage>
      {photos == null ? (
        <LinearProgress />
      ) : (
        <React.Fragment>
          <Stack
            alignItems="flex-start"
            justifyContent="space-between"
            gap={2}
            direction="row"
            marginBottom={1}
          >
            <Typography>
              {articleTitle}: {state.title}
            </Typography>
            <Stack direction="column" alignItems="flex-end" spacing={0.5}>
              <FileUploadButton
                variant="contained"
                startIcon={<FileUploadIcon />}
                maxFiles={5}
                maxFileSize={104857600}
                onFileInvalid={(values, file) => {
                  if (file == null) {
                    app.notifier.alert(photoUploadError1);
                  } else {
                    app.notifier.alert(photoUploadError2.format(file.name));
                  }
                }}
                onUploadFiles={(files) => {
                  app.articleApi.uploadPhotos(id, files).then((result) => {
                    if (result == null) return;
                    if (result.ok) {
                      loadData();
                    } else {
                      app.alertResult(result);
                    }
                  });
                }}
                inputProps={{
                  multiple: true,
                  accept: 'image/*'
                }}
              >
                {photoUpload}
              </FileUploadButton>
              <Typography variant="caption">{photoUploadTip}</Typography>
            </Stack>
          </Stack>
          <ImageList
            variant="masonry"
            cols={app.smDown ? 1 : app.mdUp ? 3 : 2}
            gap={8}
            sx={{ flexDirection: 'row' }}
          >
            <DnDList<GalleryPhotoListItem>
              items={photos}
              keyField="id"
              labelField="url"
              onDragEnd={(items) => {
                const ids = items.map((item) => item.id);
                app.articleApi.sortPhotos({ id, ids }, { showLoading: false });
              }}
              getItemStyle={(index, isDragging) => {
                return {};
              }}
              itemRenderer={(photo, index, nodeRef, actionNodeRef) => (
                <ImageListItem {...nodeRef}>
                  <img
                    src={`${photo.url}`}
                    style={{ minHeight: '80px' }}
                    alt={photo.title ?? photo.url}
                    title={photo.description}
                    loading="lazy"
                  />
                  <ImageListItemBar
                    title={`${index + 1}${
                      photo.title ? `. ${photo.title}` : ''
                    }`}
                    subtitle={`${photo.width} x ${photo.height}`}
                    actionIcon={
                      <HBox gap={0.5}>
                        <IconButton
                          title={deleteLabel}
                          size="small"
                          color="warning"
                          onClick={() => {
                            app.notifier.confirm(
                              deleteConfirm.format(photo.id.toString()),
                              undefined,
                              async (ok) => {
                                if (!ok) return;

                                const result = await app.articleApi.deletePhoto(
                                  { id, url: photo.url }
                                );
                                if (result == null) return;

                                if (result.ok) {
                                  await loadData();
                                  return;
                                }

                                app.alertResult(result);
                              }
                            );
                          }}
                        >
                          <DeleteIcon />
                        </IconButton>
                        <IconButton
                          size="small"
                          color="success"
                          title={edit}
                          onClick={() => editPhoto(photo)}
                        >
                          <EditIcon />
                        </IconButton>
                        <IconButton
                          style={{ cursor: 'move' }}
                          size="small"
                          color="info"
                          title={dragIndicator}
                          {...actionNodeRef}
                        >
                          <DragIndicatorIcon />
                        </IconButton>
                      </HBox>
                    }
                  />
                </ImageListItem>
              )}
            ></DnDList>
          </ImageList>
        </React.Fragment>
      )}
    </CommonPage>
  );
}

export default ArticleGallery;
