import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  FileUploadButton,
  Switch,
  OptionBool,
  VBox,
  TooltipClick
} from '@etsoo/materialui';
import {
  BoxProps,
  Button,
  IconButton,
  TextField,
  Typography
} from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from '@etsoo/react';
import { DriveQueryDto } from '../../api/dto/drive/DriveQueryDto';
import FileUploadIcon from '@mui/icons-material/FileUpload';
import EditIcon from '@mui/icons-material/Edit';
import DownloadIcon from '@mui/icons-material/Download';
import DeleteIcon from '@mui/icons-material/Delete';
import ShareIcon from '@mui/icons-material/Share';
import { DomUtils, NumberUtils, Utils } from '@etsoo/shared';
import { UserRole } from '@etsoo/appscript';

function AllUsers() {
  // Labels
  const labels = app.getLabels(
    'id',
    'creation',
    'actions',
    'upload',
    'driveUploadTip',
    'uploadError1',
    'uploadError2',
    'author',
    'fileName',
    'fileSize',
    'shared',
    'edit',
    'download',
    'noChanges',
    'delete',
    'deleteConfirm',
    'share',
    'removeShare',
    'shareHours',
    'copy',
    'completeTip',
    'link'
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<DriveQueryDto>>();

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  const margin = MUGlobal.pagePaddings;

  const deleteFile = (data: DriveQueryDto) => {
    app.notifier.confirm(
      labels.deleteConfirm.format(data.name),
      undefined,
      async (ok) => {
        if (!ok) return;

        const result = await app.driveApi.delete(data.id);
        if (result == null) return;

        if (result.ok) {
          await reloadData();
          return;
        }

        app.alertResult(result);
      }
    );
  };

  const editFile = (data: DriveQueryDto) => {
    app.showInputDialog({
      title: labels.edit,
      message: '',
      fullScreen: app.smDown,
      inputs: (
        <VBox paddingTop={1} paddingBottom={1} gap={2}>
          <TextField
            name="name"
            required
            label={labels.fileName}
            fullWidth
            multiline
            rows={2}
            defaultValue={data.name}
            inputProps={{ maxLength: 256 }}
          />
          <OptionBool
            name="shared"
            label={labels.shared}
            variant="outlined"
            defaultValue={data.shared.toString()}
            fullWidth
          />
          <Switch label={labels.removeShare} name="removeShare" />
        </VBox>
      ),
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const { name, shared, removeShare } = DomUtils.dataAs(
          new FormData(form),
          {
            name: 'string',
            shared: 'boolean',
            removeShare: 'boolean'
          }
        );

        // Validation
        if (!name) {
          DomUtils.setFocus('name', form);
          return false;
        }

        const rq = { id: data.id, name, shared, removeShare };
        const fields: string[] = Utils.getDataChanges(
          rq,
          data,
          removeShare ? ['id'] : ['id', 'removeShare']
        );
        if (fields.length === 0) {
          return labels.noChanges;
        }

        // Submit
        const result = await app.driveApi.update(
          { ...rq, changedFields: fields },
          {
            showLoading: false // default will show the loading bar and cause the dialog closed
          }
        );
        if (result == null) return;

        if (result.ok) {
          await reloadData();
          return;
        }

        app.alertResult(result);
      }
    });
  };

  const shareFile = (data: DriveQueryDto) => {
    if (!data.shared) {
      app.notifier.prompt(
        labels.shareHours,
        async (result) => {
          if (!result) return false;
          const hours = NumberUtils.parse(result) ?? 0;
          if (hours < 1 || hours > 720) return false;
          await doShareFile(data.id, hours);
        },
        labels.share,
        {
          type: 'number',
          inputProps: { defaultValue: 3, inputProps: { min: 1, max: 720 } }
        }
      );
    } else {
      doShareFile(data.id);
    }
  };

  const doShareFile = async (id: string, hours?: number) => {
    const result = await app.driveApi.shareFile(
      { id, hours },
      { showLoading: false }
    );
    if (result == null) return;

    const url = result.data?.id;
    if (result.ok && url) {
      app.notifier.succeed(labels.link, undefined, undefined, 300, {
        inputs: (
          <VBox gap={1} alignItems="center">
            <Typography sx={{ wordBreak: 'break-all' }}>{url}</Typography>
            <TooltipClick title={labels.completeTip.format(labels.copy)}>
              {(openTooltip) => (
                <Button
                  variant="outlined"
                  size="small"
                  onClick={() => {
                    navigator.clipboard?.writeText(url);
                    openTooltip();
                  }}
                >
                  {labels.copy}
                </Button>
              )}
            </TooltipClick>
          </VBox>
        )
      });
      return;
    }

    app.alertResult(result);
  };

  const downloadFile = async (id: string) => {
    const result = await app.driveApi.downloadFile(id);
    if (result == null) return;
    await app.download(...result);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('onlineDrive');
  }, []);

  return (
    <ResponsivePage<DriveQueryDto>
      mRef={ref}
      defaultOrderBy="creation"
      pageProps={{
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <FileUploadButton
              variant="contained"
              startIcon={<FileUploadIcon />}
              maxFiles={5}
              maxFileSize={52428800}
              title={labels.driveUploadTip}
              onFileInvalid={(values, file) => {
                if (file == null) {
                  app.notifier.alert(labels.uploadError1);
                } else {
                  app.notifier.alert(labels.uploadError2.format(file.name));
                }
              }}
              onUploadFiles={(files) => {
                app.driveApi.uploadFiles(files).then((result) => {
                  if (result == null) return;
                  if (result.ok) {
                    reloadData();
                  } else {
                    app.alertResult(result);
                  }
                });
              }}
              inputProps={{
                multiple: true
              }}
            >
              {labels.upload}
            </FileUploadButton>
          </React.Fragment>
        )
      }}
      fieldTemplate={{ shared: 'boolean' }}
      fields={[
        <SearchField label={labels.id} name="id" />,
        <SearchField label={labels.fileName} name="name" />,
        <SearchField
          label={labels.creation}
          name="creationStart"
          type="date"
        />,
        <SearchField name="creationEnd" type="date" />,
        <Switch label={labels.shared} name="shared" />
      ]}
      loadData={async (data) =>
        app.driveApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: 'name',
          header: labels.fileName,
          sortable: true
        },
        {
          field: 'size',
          header: labels.fileSize,
          width: 108,
          valueFormatter: ({ data }) => {
            if (data == null) return;
            return NumberUtils.formatFileSize(data.size);
          },
          align: 'right',
          sortable: false
        },
        {
          field: 'author',
          header: labels.author,
          sortable: false,
          width: 116
        },
        {
          field: 'creation',
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: 192,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<DriveQueryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '6px!important',
              paddingBottom: '6px!important'
            };

            return (
              <React.Fragment>
                <IconButton title={labels.edit} onClick={() => editFile(data)}>
                  <EditIcon />
                </IconButton>
                <IconButton
                  title={
                    data.shared
                      ? [labels.shared, labels.share].join(', ')
                      : labels.share
                  }
                  onClick={() => shareFile(data)}
                >
                  <ShareIcon color={data.shared ? undefined : 'error'} />
                </IconButton>
                {adminPermission && (
                  <IconButton
                    title={labels.delete}
                    onClick={() => deleteFile(data)}
                  >
                    <DeleteIcon />
                  </IconButton>
                )}
                <IconButton
                  title={labels.download}
                  onClick={() => downloadFile(data.id)}
                >
                  <DownloadIcon />
                </IconButton>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[98, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            app.formatDate(data.creation, 'd') +
              ', ' +
              NumberUtils.formatFileSize(data.size),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: () => editFile(data)
              },
              {
                label: labels.share,
                icon: <ShareIcon color={data.shared ? undefined : 'error'} />,
                action: () => shareFile(data)
              },
              adminPermission && {
                label: labels.delete,
                icon: <DeleteIcon />,
                action: () => deleteFile(data)
              },
              {
                label: labels.download,
                icon: <DownloadIcon />,
                action: () => downloadFile(data.id)
              }
            ],
            <React.Fragment>
              {data.author}
              {data.shared ? `, ${labels.shared}` : undefined}
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default AllUsers;
