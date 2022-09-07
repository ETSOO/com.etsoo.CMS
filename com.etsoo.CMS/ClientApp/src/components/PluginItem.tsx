import { IActionResult } from '@etsoo/appscript';
import { DataTypes, DomUtils, Utils } from '@etsoo/shared';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import HelpIcon from '@mui/icons-material/Help';
import { Box, Grid, IconButton, Paper, Typography } from '@mui/material';
import React from 'react';
import { app } from '../app/MyApp';
import { PlugEditDto, PluginDto } from '../dto/PluginDto';
import { PluginUpdateDto } from '../dto/PluginUpdateDto';

export interface PluginProps {
  /**
   * Plugin init items
   */
  initData?: PluginDto[];

  /**
   * Add/Edit button disabled
   */
  disabled?: boolean;
}

/**
 * Plugin item props
 */
export interface PluginItemProps {
  /**
   * Name
   */
  name: string;

  /**
   * Label
   */
  label?: string;

  /**
   * Plugin init data
   */
  initData?: PluginDto;

  /**
   * Inputs layout
   */
  inputs: (data?: PluginDto) => React.ReactNode;

  /**
   * URL of the product
   */
  url: string;

  /**
   * Add/Edit button disabled
   */
  disabled?: boolean;

  /**
   * Custom validator
   */
  validator?: (
    form: HTMLFormElement,
    data: PlugEditDto,
    editing: boolean
  ) => boolean | void;
}

/**
 * Plugin item
 * @param props Props
 * @returns Component
 */
export function PluginItem(props: PluginItemProps) {
  // Destruct
  const {
    name,
    label = app.get<string>(`service${name}`),
    initData,
    disabled,
    url,
    inputs,
    validator
  } = props;

  // State
  const [data, setData] = React.useState<PluginDto>();

  // Labels
  const labels = app.getLabels('add', 'edit', 'help', 'noChanges');

  const addService = (editing: boolean) => {
    app.showInputDialog({
      title: editing ? labels.edit : labels.add,
      message: label,
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const fd = DomUtils.dataAs(new FormData(form), {
          app: 'string',
          secret: 'string',
          enabled: 'boolean'
        });

        if (fd.enabled == null) fd.enabled = false;

        if (fd.app == null) {
          DomUtils.setFocus('app', form);
          return false;
        }

        if (validator) {
          var appData = { app: fd.app, secret: fd.secret };
          if (validator(form, appData, editing) === false) {
            return false;
          } else {
            fd.app = appData.app;
            fd.secret = appData.secret;
          }
        }

        // Request data
        type EditData = DataTypes.AddOrEditType<
          PluginUpdateDto,
          typeof editing
        >;
        const rq: EditData = {
          deviceId: app.deviceId,
          id: name,
          app: fd.app,
          enabled: fd.enabled ?? true,
          secret: fd.secret ? app.encrypt(fd.secret) : undefined
        };

        if (editing) {
          // Changed fields
          const fields: string[] = Utils.getDataChanges(fd, data!);
          if (fields.length === 0) {
            return labels.noChanges;
          }
          rq.changedFields = fields;
        }

        // Submit
        const result = await app.api.put<IActionResult>(
          editing ? 'Website/UpdateService' : 'Website/CreateService',
          rq,
          {
            showLoading: false // default will show the loading bar and cause the dialog closed
          }
        );
        if (result == null) return;

        if (result.ok) {
          var newData: PluginDto = {
            id: name,
            app: fd.app ?? data?.app,
            enabled: fd.enabled ?? data?.enabled ?? true,
            refreshTime: new Date()
          };
          setData(newData);

          return;
        }

        return app.formatResult(result);
      },
      inputs: inputs(data)
    });
  };

  React.useEffect(() => {
    setData(initData);
  }, [initData]);

  return (
    <Grid item xs={12} sm={6} lg={4}>
      <Paper
        sx={{
          padding: 2,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}
      >
        <Box flexGrow={2}>
          <Typography
            sx={{
              textDecoration:
                data == null || data.enabled ? 'unset' : 'line-through'
            }}
          >
            {label}
          </Typography>
          <Typography variant="caption">
            {name}
            {data?.refreshTime ? ', ' + app.formatDate(data.refreshTime) : ''}
          </Typography>
        </Box>
        <Box gap={1}>
          {data?.app ? (
            <IconButton
              title={labels.edit}
              color="primary"
              onClick={() => addService(true)}
              disabled={disabled}
            >
              <EditIcon />
            </IconButton>
          ) : (
            <IconButton
              title={labels.add}
              color="primary"
              onClick={() => addService(false)}
              disabled={disabled}
            >
              <AddIcon />
            </IconButton>
          )}
          <IconButton
            title={labels.help}
            size="small"
            href={url}
            target="_blank"
          >
            <HelpIcon />
          </IconButton>
        </Box>
      </Paper>
    </Grid>
  );
}
